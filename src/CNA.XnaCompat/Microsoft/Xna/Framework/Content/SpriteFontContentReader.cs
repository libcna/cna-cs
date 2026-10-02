namespace Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// XNA's <c>SpriteFontReader</c>, for a font read from a stream a game's own content manager opens
/// (<see cref="ResourceContentManager"/>, CSX-129). A font loaded from a file goes to CNA's own
/// loader instead. The order is XNA's (IL): the atlas, glyph bounds, cropping, characters, line
/// spacing, spacing, kerning, then a flag and the default character.
/// </summary>
internal sealed class SpriteFontContentReader : ContentTypeReader<SpriteFont>
{
    protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
    {
        ArgumentNullException.ThrowIfNull(input);

        Texture2D texture = input.ReadObject<Texture2D>();
        List<Rectangle> glyphBounds = input.ReadObject<List<Rectangle>>();
        List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
        List<char> characters = input.ReadObject<List<char>>();
        int lineSpacing = input.ReadInt32();
        float spacing = input.ReadSingle();
        List<Vector3> kerning = input.ReadObject<List<Vector3>>();
        char? defaultCharacter = input.ReadBoolean() ? input.ReadChar() : null;

        return new SpriteFont(texture, glyphBounds, cropping, characters, lineSpacing, spacing, kerning, defaultCharacter);
    }
}
