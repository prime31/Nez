using System.Collections.Generic;

namespace Nez
{
    /// <summary>
    /// Used with a FollowCamera in order to achieve a parallax effect.
    /// </summary>
    public class ParallaxLayer
    {
        /// <summary>
        /// Contains every entity that should be affected by this layer.
        /// </summary>
        public List<Entity> entities { get; set; } = new List<Entity>();

        /// <summary>
        /// The amount of parallax to take into account.  A value of 0 is completely still, whereas a value of 1 moves in tandem with the FollowCamera
        /// </summary>
        public float parallaxFactor { get; set; }

        /// <summary>
        /// Initialize the ParallaxLayer
        /// </summary>
        /// <param name="ParallaxFactor">The amount of parallax to take into account.  Normal values from 0 - 1, where 1 is full movement, and 0 is completely still.</param>
        /// <param name="Entities">The entities to initialize this layer with</param>
        public ParallaxLayer(float ParallaxFactor, params Entity[] Entities)
        {
            parallaxFactor = ParallaxFactor;
            entities.AddRange(Entities);
        }
    }
}