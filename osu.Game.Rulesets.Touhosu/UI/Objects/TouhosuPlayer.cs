﻿using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osuTK;
using System;
using osuTK.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Graphics.Shapes;
using System.Collections.Generic;
using osu.Game.Rulesets.UI;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Touhosu.Extensions;

namespace osu.Game.Rulesets.Touhosu.UI.Objects
{
    public class TouhosuPlayer : CompositeDrawable, IKeyBindingHandler<TouhosuAction>
    {
        private const float base_speed = 0.2f;
        private const float shoot_delay = 80;

        private float speedMultiplier = 1;

        private readonly Bindable<PlayerState> state = new Bindable<PlayerState>(PlayerState.Idle);

        private HitObjectContainer hitObjects;

        public HitObjectContainer HitObjects
        {
            get => hitObjects;
            set
            {
                hitObjects = value;
                cardsController.HitObjects = value;
            }
        }

        public override bool RemoveCompletedTransforms => false;

        private int horizontalDirection;
        private int verticalDirection;

        public readonly Container Player;
        private readonly FocusAnimation focus;
        private readonly CardsController cardsController;
        private readonly Container animationContainer;

        public TouhosuPlayer()
        {
            RelativeSizeAxes = Axes.Both;
            AddRangeInternal(new Drawable[]
            {
                cardsController = new CardsController(),
                Player = new Container
                {
                    Origin = Anchor.Centre,
                    Position = new Vector2(TouhosuPlayfield.ACTUAL_SIZE.X / 2f, TouhosuPlayfield.ACTUAL_SIZE.Y - 20),
                    Children = new Drawable[]
                    {
                        focus = new FocusAnimation(),
                        animationContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(23.25f, 33.75f),
                        },
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(3),
                            Colour = Color4.Red,
                        },
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(2),
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            state.BindValueChanged(onStateChanged, true);
        }

        private bool isDead;

        public void Die()
        {
            isDead = true;
            onFocusReleased();
            onShootReleased();
            Player.FadeOut(500, Easing.Out);
        }

        public void PlayMissAnimation()
        {
            if (isDead)
                return;

            animationContainer.FlashColour(Color4.Red, 1000, Easing.OutQuint);
        }

        public Vector2 PlayerPosition() => Player.Position;

        public bool OnPressed(TouhosuAction action)
        {
            if (isDead)
                return true;

            switch (action)
            {
                case TouhosuAction.MoveLeft:
                    horizontalDirection--;
                    return true;

                case TouhosuAction.MoveRight:
                    horizontalDirection++;
                    return true;

                case TouhosuAction.MoveUp:
                    verticalDirection--;
                    return true;

                case TouhosuAction.MoveDown:
                    verticalDirection++;
                    return true;

                case TouhosuAction.Focus:
                    onFocusPressed();
                    return true;

                case TouhosuAction.Shoot:
                    onShootPressed();
                    return true;
            }

            return false;
        }

        public void OnReleased(TouhosuAction action)
        {
            if (isDead)
                return;

            switch (action)
            {
                case TouhosuAction.MoveLeft:
                    horizontalDirection++;
                    return;

                case TouhosuAction.MoveRight:
                    horizontalDirection--;
                    return;

                case TouhosuAction.MoveUp:
                    verticalDirection++;
                    return;

                case TouhosuAction.MoveDown:
                    verticalDirection--;
                    return;

                case TouhosuAction.Focus:
                    onFocusReleased();
                    return;

                case TouhosuAction.Shoot:
                    onShootReleased();
                    return;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isDead)
                return;

            move(Clock.ElapsedFrameTime, horizontalDirection, verticalDirection);
            updatePlayerState();
        }

        private void move(double elapsedTime, int horizontalDirection, int verticalDirection)
        {
            var movingH = horizontalDirection != 0;
            var movingV = verticalDirection != 0;

            if (!movingV && !movingH)
                return;

            // Diagonal movement
            if (movingV && movingH)
            {
                var oldX = Player.X;
                var oldY = Player.Y;
                var newX = oldX + Math.Sign(horizontalDirection) * elapsedTime * base_speed * speedMultiplier;
                var newY = oldY + Math.Sign(verticalDirection) * elapsedTime * base_speed * speedMultiplier;

                var expectedDistance = Math.Abs(newX - oldX);
                var realDistance = MathExtensions.Distance(new Vector2(oldX, oldY), new Vector2((float)newX, (float)newY));
                var offset = Math.Sqrt(MathExtensions.Pow(expectedDistance - realDistance) / 2);

                newX += (horizontalDirection > 0 ? -1 : 1) * offset;
                newY += (verticalDirection > 0 ? -1 : 1) * offset;

                newX = Math.Clamp(newX, animationContainer.Width / 2, TouhosuPlayfield.ACTUAL_SIZE.X - animationContainer.Width / 2);
                newY = Math.Clamp(newY, animationContainer.Height / 2, TouhosuPlayfield.ACTUAL_SIZE.Y - animationContainer.Height / 2);

                Player.Position = new Vector2((float)newX, (float)newY);
                return;
            }

            if (movingV)
            {
                var position = Math.Clamp(Player.Y + Math.Sign(verticalDirection) * elapsedTime * base_speed * speedMultiplier, animationContainer.Height / 2, TouhosuPlayfield.ACTUAL_SIZE.Y - animationContainer.Height / 2);
                Player.Y = (float)position;
                return;
            }

            if (movingH)
            {
                var position = Math.Clamp(Player.X + Math.Sign(horizontalDirection) * elapsedTime * base_speed * speedMultiplier, animationContainer.Width / 2, TouhosuPlayfield.ACTUAL_SIZE.X - animationContainer.Width / 2);
                Player.X = (float)position;
            }
        }

        public List<Card> GetCards() => cardsController.GetCards();

        private bool isFocused;

        private void onFocusPressed()
        {
            isFocused = true;
            speedMultiplier = 0.5f;
            focus.Focus();
        }

        private void onFocusReleased()
        {
            isFocused = false;
            speedMultiplier = 1;
            focus.FocusLost();
        }

        private void onShootPressed()
        {
            cardsController.Shoot(PlayerPosition(), isFocused);
            Scheduler.AddDelayed(onShootPressed, shoot_delay);
        }

        private void onShootReleased()
        {
            Scheduler.CancelDelayedTasks();
        }

        private void updatePlayerState()
        {
            if (horizontalDirection == 1)
            {
                state.Value = PlayerState.Right;
                return;
            }

            if (horizontalDirection == -1)
            {
                state.Value = PlayerState.Left;
                return;
            }

            state.Value = PlayerState.Idle;
        }

        private void onStateChanged(ValueChangedEvent<PlayerState> s)
        {
            animationContainer.Child = new PlayerAnimation(s.NewValue);
        }
    }
}
