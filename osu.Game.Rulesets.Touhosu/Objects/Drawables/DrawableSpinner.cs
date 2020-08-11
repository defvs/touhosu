﻿namespace osu.Game.Rulesets.Touhosu.Objects.Drawables
{
    public class DrawableSpinner : DrawableGroupedTouhosuHitObject<AngeledProjectile, Spinner>
    {
        public DrawableSpinner(Spinner h)
            : base(h)
        {
        }

        protected override DrawableProjectile CreateDrawableProjectile(AngeledProjectile projectile) => new DrawableAngeledProjectile(projectile);
    }
}
