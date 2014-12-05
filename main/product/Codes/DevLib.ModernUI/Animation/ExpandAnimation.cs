//-----------------------------------------------------------------------
// <copyright file="ExpandAnimation.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Animation
{
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// ExpandAnimation class.
    /// </summary>
    public sealed class ExpandAnimation : AnimationBase
    {
        /// <summary>
        /// Start animation.
        /// </summary>
        /// <param name="control">Control to use.</param>
        /// <param name="targetSize">Target size.</param>
        /// <param name="transitionType">TransitionType instance.</param>
        /// <param name="duration">Duration time in milliseconds.</param>
        public void Start(Control control, Size targetSize, TransitionType transitionType, int duration)
        {
            base.Start(
                control,
                transitionType,
                duration,
                delegate
                {
                    int width = this.DoExpandAnimation(control.Width, targetSize.Width);
                    int height = this.DoExpandAnimation(control.Height, targetSize.Height);

                    control.Size = new Size(width, height);
                },
                delegate
                {
                    return control.Size.Equals(targetSize);
                });
        }

        /// <summary>
        /// DoExpandAnimation method.
        /// </summary>
        /// <param name="startSize">Start size.</param>
        /// <param name="targetSize">Target size.</param>
        /// <returns>Transition result.</returns>
        private int DoExpandAnimation(int startSize, int targetSize)
        {
            float t = (float)this.Counter - (float)this.StartTime;
            float b = (float)startSize;
            float c = (float)targetSize - (float)startSize;
            float d = (float)this.TargetTime - (float)this.StartTime;

            return this.MakeTransition(t, b, d, c);
        }
    }
}