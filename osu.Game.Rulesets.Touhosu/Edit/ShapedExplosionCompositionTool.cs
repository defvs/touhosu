﻿using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Touhosu.Edit.Blueprints;
using osu.Game.Rulesets.Touhosu.Objects;

namespace osu.Game.Rulesets.Touhosu.Edit
{
    public class ShapedExplosionCompositionTool : HitObjectCompositionTool
    {
        public ShapedExplosionCompositionTool()
            : base(nameof(ShapedExplosion))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new ShapedExplosionPlacementBlueprint();
    }
}