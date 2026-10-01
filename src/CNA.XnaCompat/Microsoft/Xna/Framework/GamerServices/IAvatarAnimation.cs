using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.GamerServices;

public interface IAvatarAnimation
{
    ReadOnlyCollection<Matrix> BoneTransforms { get; }

    TimeSpan CurrentPosition { get; set; }

    TimeSpan Length { get; }

    AvatarExpression Expression { get; }

    void Update(TimeSpan elapsedAnimationTime, bool loop);
}
