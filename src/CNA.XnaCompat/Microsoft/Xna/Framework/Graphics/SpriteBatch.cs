using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>XNA 4.0 SpriteBatch facade. Its public base is GraphicsResource; the CNA batch remains
/// a private implementation object and owns the one native batch handle.</summary>
public class SpriteBatch : GraphicsResource
{
    private readonly CNA.Graphics.SpriteBatch _inner;
    private SpriteSortMode _sortMode;
    private BlendState? _blendState;
    private SamplerState? _samplerState;
    private DepthStencilState? _depthStencilState;
    private RasterizerState? _rasterizerState;

    public SpriteBatch(GraphicsDevice graphicsDevice)
        : base(graphicsDevice)
    {
        _inner = new CNA.Graphics.SpriteBatch(graphicsDevice.Framework);
    }

    public void Begin()
    {
        _inner.Begin();
        Begun(SpriteSortMode.Deferred, null, null, null, null);
    }

    public void Begin(SpriteSortMode sortMode, BlendState? blendState)
    {
        _inner.Begin((CNA.Graphics.SpriteSortMode)(int)sortMode, blendState?.Framework);
        Begun(sortMode, blendState, null, null, null);
    }

    public void Begin(
        SpriteSortMode sortMode,
        BlendState? blendState,
        SamplerState? samplerState,
        DepthStencilState? depthStencilState,
        RasterizerState? rasterizerState)
    {
        _inner.Begin(
            (CNA.Graphics.SpriteSortMode)(int)sortMode,
            blendState?.Framework,
            samplerState?.Framework,
            depthStencilState?.Framework,
            rasterizerState?.Framework);
        Begun(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
    }

    public void Begin(
        SpriteSortMode sortMode,
        BlendState? blendState,
        SamplerState? samplerState,
        DepthStencilState? depthStencilState,
        RasterizerState? rasterizerState,
        Effect? effect)
    {
        _inner.Begin(
            (CNA.Graphics.SpriteSortMode)(int)sortMode,
            blendState?.Framework,
            samplerState?.Framework,
            depthStencilState?.Framework,
            rasterizerState?.Framework,
            effect?.Inner);
        Begun(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
    }

    public void Begin(
        SpriteSortMode sortMode,
        BlendState? blendState,
        SamplerState? samplerState,
        DepthStencilState? depthStencilState,
        RasterizerState? rasterizerState,
        Effect? effect,
        Matrix transformMatrix)
    {
        _inner.Begin(
            (CNA.Graphics.SpriteSortMode)(int)sortMode,
            blendState?.Framework,
            samplerState?.Framework,
            depthStencilState?.Framework,
            rasterizerState?.Framework,
            effect?.Inner,
            transformMatrix.ToFramework());
        Begun(sortMode, blendState, samplerState, depthStencilState, rasterizerState);
    }

    public void End()
    {
        _inner.End();
        if (_sortMode != SpriteSortMode.Immediate)
        {
            NoteRenderState();
        }
    }

    /// <summary>
    /// Remembers the states this batch applies, null meaning XNA's default for the slot. Native
    /// applies them where XNA's <c>SetRenderState</c> does -- at Begin for Immediate, at End for
    /// every other mode, an empty batch included -- and the device's cache is told at the same point.
    /// </summary>
    private void Begun(
        SpriteSortMode sortMode,
        BlendState? blendState,
        SamplerState? samplerState,
        DepthStencilState? depthStencilState,
        RasterizerState? rasterizerState)
    {
        _sortMode = sortMode;
        _blendState = blendState ?? BlendState.AlphaBlend;
        _samplerState = samplerState ?? SamplerState.LinearClamp;
        _depthStencilState = depthStencilState ?? DepthStencilState.None;
        _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
        if (sortMode == SpriteSortMode.Immediate)
        {
            NoteRenderState();
        }
    }

    private void NoteRenderState() =>
        GraphicsDevice.NoteSpriteBatchRenderState(_blendState!, _depthStencilState!, _rasterizerState!, _samplerState!);

    public void Draw(Texture2D texture, Vector2 position, Color color) =>
        _inner.Draw(Backend(texture), position.ToFramework(), color.ToFramework());

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) =>
        _inner.Draw(
            Backend(texture), position.ToFramework(), sourceRectangle.ToFramework(), color.ToFramework());

    public void Draw(
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth) =>
        _inner.Draw(
            Backend(texture),
            position.ToFramework(),
            sourceRectangle.ToFramework(),
            color.ToFramework(),
            rotation,
            origin.ToFramework(),
            scale,
            (CNA.Graphics.SpriteEffects)(int)effects, layerDepth);

    public void Draw(
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth) =>
        _inner.Draw(
            Backend(texture),
            position.ToFramework(),
            sourceRectangle.ToFramework(),
            color.ToFramework(),
            rotation,
            origin.ToFramework(),
            scale.ToFramework(),
            (CNA.Graphics.SpriteEffects)(int)effects, layerDepth);

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color) =>
        _inner.Draw(Backend(texture), destinationRectangle.ToFramework(), color.ToFramework());

    public void Draw(
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
        Color color) =>
        _inner.Draw(
            Backend(texture),
            destinationRectangle.ToFramework(),
            sourceRectangle.ToFramework(),
            color.ToFramework());

    public void Draw(
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        SpriteEffects effects,
        float layerDepth) =>
        _inner.Draw(
            Backend(texture),
            destinationRectangle.ToFramework(),
            sourceRectangle.ToFramework(),
            color.ToFramework(),
            rotation,
            origin.ToFramework(),
            (CNA.Graphics.SpriteEffects)(int)effects, layerDepth);

    public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        _inner.DrawString(spriteFont.Framework, text, position.ToFramework(), color.ToFramework());
    }

    public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        _inner.DrawString(spriteFont.Framework, text.ToString(), position.ToFramework(), color.ToFramework());
    }

    public void DrawString(
        SpriteFont spriteFont,
        string text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        _inner.DrawString(
            spriteFont.Framework,
            text,
            position.ToFramework(),
            color.ToFramework(),
            rotation,
            origin.ToFramework(),
            scale,
            (CNA.Graphics.SpriteEffects)(int)effects, layerDepth);
    }

    public void DrawString(
        SpriteFont spriteFont,
        StringBuilder text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        DrawString(spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, layerDepth);
    }

    public void DrawString(
        SpriteFont spriteFont,
        string text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        _inner.DrawString(
            spriteFont.Framework,
            text,
            position.ToFramework(),
            color.ToFramework(),
            rotation,
            origin.ToFramework(),
            scale.ToFramework(),
            (CNA.Graphics.SpriteEffects)(int)effects, layerDepth);
    }

    public void DrawString(
        SpriteFont spriteFont,
        StringBuilder text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        DrawString(spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, layerDepth);
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        try
        {
            _inner?.Dispose();
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    private static CNA.Graphics.Texture2D Backend(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        return (CNA.Graphics.Texture2D)texture.FrameworkTexture;
    }

    private static CNA.Graphics.SpriteFont Backend(SpriteFont spriteFont)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        return spriteFont.Framework;
    }
}
