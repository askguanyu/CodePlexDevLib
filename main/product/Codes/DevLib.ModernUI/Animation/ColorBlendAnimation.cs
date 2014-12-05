//-----------------------------------------------------------------------
// <copyright file="ColorBlendAnimation.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Animation
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;

    /// <summary>
    /// ColorBlendAnimation class.
    /// </summary>
    public sealed class ColorBlendAnimation : AnimationBase
    {
        /// <summary>
        /// Field _percent;
        /// </summary>
        private double _percent = 1;

        /// <summary>
        /// Start animation.
        /// </summary>
        /// <param name="control">Control to use.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="targetColor">Target color.</param>
        /// <param name="duration">Duration time in milliseconds.</param>
        public void Start(Control control, string propertyName, Color targetColor, int duration)
        {
            if (duration == 0)
            {
                duration = 1;
            }

            base.Start(
                control,
                this.TransitionType,
                2 * duration,
                delegate
                {
                    Color controlColor = this.GetPropertyValue(propertyName, control);
                    Color newColor = this.DoColorBlend(controlColor, targetColor, 0.1 * (this._percent / 2));

                    PropertyInfo propertyInfo = control.GetType().GetProperty(propertyName);
                    MethodInfo methodInfo = propertyInfo.GetSetMethod(true);

                    methodInfo.Invoke(control, new object[] { newColor });
                },
                delegate
                {
                    Color controlColor = this.GetPropertyValue(propertyName, control);

                    if (controlColor.A.Equals(targetColor.A) &&
                        controlColor.R.Equals(targetColor.R) &&
                        controlColor.G.Equals(targetColor.G) &&
                        controlColor.B.Equals(targetColor.B))
                    {
                        return true;
                    }

                    return false;
                });
        }

        /// <summary>
        /// DoColorBlend method.
        /// </summary>
        /// <param name="startColor">Start color.</param>
        /// <param name="targetColor">Target color.</param>
        /// <param name="ratio">Ratio value.</param>
        /// <returns>Color instance.</returns>
        private Color DoColorBlend(Color startColor, Color targetColor, double ratio)
        {
            this._percent += 0.2;

            int a = (int)Math.Round((startColor.A * (1 - ratio)) + (targetColor.A * ratio));
            int r = (int)Math.Round((startColor.R * (1 - ratio)) + (targetColor.R * ratio));
            int g = (int)Math.Round((startColor.G * (1 - ratio)) + (targetColor.G * ratio));
            int b = (int)Math.Round((startColor.B * (1 - ratio)) + (targetColor.B * ratio));

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Get property value.
        /// </summary>
        /// <param name="propertyName">Property name to get.</param>
        /// <param name="control">Control to get.</param>
        /// <returns>Color instance.</returns>
        private Color GetPropertyValue(string propertyName, Control control)
        {
            Type type = control.GetType();

            object value = type.InvokeMember(propertyName, BindingFlags.GetProperty, null, control, null);

            return (Color)value;
        }
    }
}