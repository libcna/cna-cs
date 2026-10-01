using System.Collections.ObjectModel;
using CNA;
using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>
/// XNA's preset avatar animation, over a CNA avatar animation handle. Like XNA's, it holds its
/// position, length, face and the 71 bone transforms as its own state, refreshed by every
/// <see cref="Update"/>: the getters keep answering after <see cref="Dispose()"/>, and
/// <see cref="BoneTransforms"/> is one collection whose contents change in place.
/// </summary>
public class AvatarAnimation : IAvatarAnimation, IDisposable
{
    private readonly NativeResourceHandle _handle;
    private readonly Matrix[] _avatarBones = new Matrix[AvatarRenderer.BoneCount];
    private readonly ReadOnlyCollection<Matrix> _boneTransforms;
    private readonly object _disposeLock = new();
    private TimeSpan _length;
    private TimeSpan _currentPosition;
    private AvatarExpression _currentExpression;
    private bool _isDisposed;

    public AvatarAnimation(AvatarAnimationPreset animationPreset)
    {
        GamerServicesInterop.Check(
            Native.cna_avatar_animation_create((uint)animationPreset, out CnaHandle handle), nameof(AvatarAnimation));
        _handle = new NativeResourceHandle(
            handle.Value, value => Native.cna_avatar_animation_destroy(new CnaHandle(value)).IsSuccess());
        _boneTransforms = new ReadOnlyCollection<Matrix>(_avatarBones);
        Refresh();
    }

    ~AvatarAnimation()
    {
        Dispose(false);
    }

    private CnaHandle Handle => new(_handle.DangerousGetHandle());

    public TimeSpan Length => _length;

    /// <summary>Setting the position re-poses the avatar there, as XNA's setter does through
    /// <c>Update(TimeSpan.Zero, false)</c>.</summary>
    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        set
        {
            ThrowIfDisposed();
            GamerServicesInterop.Check(
                Native.cna_avatar_animation_set_current_position(Handle, value.Ticks), nameof(CurrentPosition));
            Refresh();
        }
    }

    public ReadOnlyCollection<Matrix> BoneTransforms => _boneTransforms;

    public AvatarExpression Expression => _currentExpression;

    public bool IsDisposed => _isDisposed;

    public void Update(TimeSpan elapsedAnimationTime, bool loop)
    {
        ThrowIfDisposed();
        GamerServicesInterop.Check(
            Native.cna_avatar_animation_update(Handle, elapsedAnimationTime.Ticks, GamerServicesInterop.Bool(loop)),
            nameof(Update));
        Refresh();
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

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    private void Refresh()
    {
        CnaAvatarAnimationInfo info = GamerServicesInterop.Versioned<CnaAvatarAnimationInfo>();
        GamerServicesInterop.Check(Native.cna_avatar_animation_get_info(Handle, ref info), nameof(AvatarAnimation));
        _length = new TimeSpan(info.LengthTicks);
        _currentPosition = new TimeSpan(info.CurrentPositionTicks);

        CnaAvatarExpression expression = GamerServicesInterop.Versioned<CnaAvatarExpression>();
        GamerServicesInterop.Check(Native.cna_avatar_animation_get_expression(Handle, ref expression), nameof(Expression));
        _currentExpression = AvatarRenderer.FromNative(expression);

        int count = Math.Min(info.BoneTransformCount, _avatarBones.Length);
        for (int index = 0; index < count; index++)
        {
            CnaMatrix transform = default;
            GamerServicesInterop.Check(
                Native.cna_avatar_animation_get_bone_transform_at(Handle, index, ref transform), nameof(BoneTransforms));
            _avatarBones[index] = GamerServicesInterop.FromNative(transform);
        }
    }
}
