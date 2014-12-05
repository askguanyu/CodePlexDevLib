//-----------------------------------------------------------------------
// <copyright file="MoveAnimation.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Animation
{
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// MoveAnimation class.
    /// </summary>
    public sealed class MoveAnimation : AnimationBase
    {
        /// <summary>
        /// Start animation.
        /// </summary>
        /// <param name="control">Control to use.</param>
        /// <param name="targetPoint">Target point.</param>
        /// <param name="transitionType">TransitionType instance.</param>
        /// <param name="duration">Duration time in milliseconds.</param>
        public void Start(Control control, Point targetPoint, TransitionType transitionType, int duration)
        {
            base.Start(
                control,
                transitionType,
                duration,
                delegate
                {
                    int x = this.DoMoveAnimation(control.Location.X, targetPoint.X);
                    int y = this.DoMoveAnimation(control.Location.Y, targetPoint.Y);

                    control.Location = new Point(x, y);
                },
                delegate
                {
                    return control.Location.Equals(targetPoint);
                });
        }

        /// <summary>
        /// DoMoveAnimation method.
        /// </summary>
        /// <param name="startPosition">Start position.</param>
        /// <param name="targetPosition">Target position.</param>
        /// <returns>Transition result.</returns>
        private int DoMoveAnimation(int startPosition, int targetPosition)
        {
            float t = (float)this.Counter - (float)this.StartTime;
            float b = (float)startPosition;
            float c = (float)targetPosition - (float)startPosition;
            float d = (float)this.TargetTime - (float)this.StartTime;

            return this.MakeTransition(t, b, d, c);
        }
    }
}