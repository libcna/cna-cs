using System.Collections.ObjectModel;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's avatar renderer, drawing through CNA's avatar renderer on the game's graphics device.
///
/// The transforms and lights are this object's own state, as XNA's are -- set freely, readable
/// after <see cref="Dispose()"/> -- starting from native's defaults and handed to native with each
/// <see cref="Draw(IList{Matrix}, AvatarExpression)"/>. A light native cannot draw with (a
/// non-finite component) is therefore refused by the draw, not by the setter.
/// </summary>
public class AvatarRenderer : IDisposable
{
#pragma warning disable CS3021 // XNA carries this member attribute even without an assembly-level CLS declaration.
    [CLSCompliant(false)]
    public const int BoneCount = 71;
#pragma warning restore CS3021

    private readonly NativeResourceHandle _handle;
    private readonly ReadOnlyCollection<int> _parentBones;
    private readonly Matrix[] _bindPoseArray = new Matrix[BoneCount];
    private readonly object _disposeLock = new();
    private ReadOnlyCollection<Matrix>? _bindPose;
    private Matrix _world;
    private Matrix _view;
    private Matrix _projection;
    private bool _isDisposed;

    public AvatarRenderer(AvatarDescription avatarDescription)
        : this(avatarDescription, useLoadingEffect: true)
    {
    }

    public AvatarRenderer(AvatarDescription avatarDescription, bool useLoadingEffect)
    {
        ArgumentNullException.ThrowIfNull(avatarDescription);
        GamerServicesInterop.Check(
            Native.cna_avatar_renderer_create(avatarDescription.Handle, GamerServicesInterop.Bool(useLoadingEffect), out CnaHandle handle),
            nameof(AvatarRenderer));
        _handle = new NativeResourceHandle(
            handle.AsNint, value => Native.cna_avatar_renderer_destroy(new CnaHandle(value)).IsSuccess());

        int[] parents = new int[BoneCount];
        for (int index = 0; index < BoneCount; index++)
        {
            GamerServicesInterop.Check(
                Native.cna_avatar_renderer_get_parent_bone_at(Handle, index, out parents[index]), nameof(ParentBones));
        }

        _parentBones = new ReadOnlyCollection<int>(parents);

        CnaMatrix world = default, view = default, projection = default;
        GamerServicesInterop.Check(
            Native.cna_avatar_renderer_get_transforms(Handle, ref world, ref view, ref projection), nameof(AvatarRenderer));
        _world = GamerServicesInterop.FromNative(world);
        _view = GamerServicesInterop.FromNative(view);
        _projection = GamerServicesInterop.FromNative(projection);

        CnaVector3 lightColor = default, lightDirection = default, ambient = default;
        GamerServicesInterop.Check(
            Native.cna_avatar_renderer_get_lighting(Handle, ref lightColor, ref lightDirection, ref ambient), nameof(AvatarRenderer));
        LightColor = GamerServicesInterop.FromNative(lightColor);
        LightDirection = GamerServicesInterop.FromNative(lightDirection);
        AmbientLightColor = GamerServicesInterop.FromNative(ambient);
    }

    ~AvatarRenderer()
    {
        Dispose(false);
    }

    private CnaHandle Handle => new(_handle.DangerousGetHandle());

    public Matrix World
    {
        get => _world;
        set => _world = value;
    }

    public Matrix View
    {
        get => _view;
        set => _view = value;
    }

    public Matrix Projection
    {
        get => _projection;
        set => _projection = value;
    }

    public ReadOnlyCollection<int> ParentBones => _parentBones;

    /// <summary>Readable once the renderer is <see cref="AvatarRendererState.Ready"/>.</summary>
    public ReadOnlyCollection<Matrix> BindPose
    {
        get
        {
            ThrowIfDisposed();
            if (State != AvatarRendererState.Ready)
            {
                throw new InvalidOperationException("The bind pose is not available until the avatar renderer is ready.");
            }

            lock (_bindPoseArray)
            {
                if (_bindPose is null)
                {
                    for (int index = 0; index < BoneCount; index++)
                    {
                        CnaMatrix transform = default;
                        GamerServicesInterop.Check(
                            Native.cna_avatar_renderer_get_bind_pose_at(Handle, index, ref transform), nameof(BindPose));
                        _bindPoseArray[index] = GamerServicesInterop.FromNative(transform);
                    }

                    _bindPose = new ReadOnlyCollection<Matrix>(_bindPoseArray);
                }
            }

            return _bindPose;
        }
    }

    public AvatarRendererState State
    {
        get
        {
            ThrowIfDisposed();
            CnaAvatarRendererInfo info = GamerServicesInterop.Versioned<CnaAvatarRendererInfo>();
            GamerServicesInterop.Check(Native.cna_avatar_renderer_get_info(Handle, ref info), nameof(State));
            return (AvatarRendererState)info.State;
        }
    }

    public Vector3 LightColor { get; set; }

    public Vector3 LightDirection { get; set; }

    public Vector3 AmbientLightColor { get; set; }

    public bool IsDisposed => _isDisposed;

    public void Draw(IAvatarAnimation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);
        Draw(animation.BoneTransforms, animation.Expression);
    }

    public unsafe void Draw(IList<Matrix> bones, AvatarExpression expression)
    {
        lock (_disposeLock)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(bones);
            if (bones.Count != BoneCount)
            {
                throw new ArgumentException($"Exactly {BoneCount} bone transforms are required.", nameof(bones));
            }

            CnaMatrix* pose = stackalloc CnaMatrix[BoneCount];
            for (int index = 0; index < BoneCount; index++)
            {
                pose[index] = GamerServicesInterop.ToNative(bones[index]);
            }

            CnaMatrix world = GamerServicesInterop.ToNative(_world);
            CnaMatrix view = GamerServicesInterop.ToNative(_view);
            CnaMatrix projection = GamerServicesInterop.ToNative(_projection);
            GamerServicesInterop.Check(Native.cna_avatar_renderer_set_transforms(Handle, &world, &view, &projection), nameof(Draw));

            CnaVector3 lightColor = GamerServicesInterop.ToNative(LightColor);
            CnaVector3 lightDirection = GamerServicesInterop.ToNative(LightDirection);
            CnaVector3 ambient = GamerServicesInterop.ToNative(AmbientLightColor);
            GamerServicesInterop.Check(Native.cna_avatar_renderer_set_lighting(Handle, &lightColor, &lightDirection, &ambient), nameof(Draw));

            CnaAvatarExpression face = ToNative(expression);
            GamerServicesInterop.Check(Native.cna_avatar_renderer_draw_bones(Handle, pose, BoneCount, &face), nameof(Draw));
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>The native handle finalizes itself, so only an explicit dispose releases it here.</summary>
    protected virtual void Dispose(bool disposing)
    {
        lock (_disposeLock)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (disposing)
            {
                _handle.Dispose();
            }
        }
    }

    internal static AvatarExpression FromNative(CnaAvatarExpression expression) => new()
    {
        Mouth = (AvatarMouth)expression.Mouth,
        LeftEye = (AvatarEye)expression.LeftEye,
        RightEye = (AvatarEye)expression.RightEye,
        LeftEyebrow = (AvatarEyebrow)expression.LeftEyebrow,
        RightEyebrow = (AvatarEyebrow)expression.RightEyebrow,
    };

    private static CnaAvatarExpression ToNative(AvatarExpression expression)
    {
        CnaAvatarExpression native = GamerServicesInterop.Versioned<CnaAvatarExpression>();
        native.Mouth = (uint)expression.Mouth;
        native.LeftEye = (uint)expression.LeftEye;
        native.RightEye = (uint)expression.RightEye;
        native.LeftEyebrow = (uint)expression.LeftEyebrow;
        native.RightEyebrow = (uint)expression.RightEyebrow;
        return native;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}
