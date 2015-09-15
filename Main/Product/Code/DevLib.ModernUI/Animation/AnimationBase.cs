//-----------------------------------------------------------------------
// <copyright file="AnimationBase.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.ModernUI.Animation
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Delegate AnimationAction.
    /// </summary>
    public delegate void AnimationAction();

    /// <summary>
    /// Delegate AnimationFinishedEvaluator.
    /// </summary>
    /// <returns>Method result.</returns>
    public delegate bool AnimationFinishedEvaluator();

    /// <summary>
    /// AnimationBase class.
    /// </summary>
    public abstract class AnimationBase
    {
        /// <summary>
        /// Field _timer.
        /// </summary>
        private DelayedCall _timer;

        /// <summary>
        /// Field _targetControl.
        /// </summary>
        private Control _targetControl;

        /// <summary>
        /// Field _actionHandler.
        /// </summary>
        private AnimationAction _actionHandler;

        /// <summary>
        /// Field _evaluatorHandler.
        /// </summary>
        private AnimationFinishedEvaluator _evaluatorHandler;

        /// <summary>
        /// Event AnimationCompleted.
        /// </summary>
        public event EventHandler AnimationCompleted;

        /// <summary>
        /// Gets a value indicating whether animation is completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (this._timer != null)
                {
                    return !this._timer.IsWaiting;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether animation is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (this._timer != null)
                {
                    return this._timer.IsWaiting;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets or sets TransitionType.
        /// </summary>
        protected TransitionType TransitionType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Counter.
        /// </summary>
        protected int Counter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets StartTime.
        /// </summary>
        protected int StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets TargetTime.
        /// </summary>
        protected int TargetTime
        {
            get;
            set;
        }

        /// <summary>
        /// Cancel animation.
        /// </summary>
        public void Cancel()
        {
            if (this.IsRunning)
            {
                this._timer.Cancel();
            }
        }

        /// <summary>
        /// Start animation.
        /// </summary>
        /// <param name="control">Control to use.</param>
        /// <param name="transitionType">TransitionType instance.</param>
        /// <param name="duration">Duration time in milliseconds.</param>
        /// <param name="actionHandler">AnimationAction delegate.</param>
        protected void Start(Control control, TransitionType transitionType, int duration, AnimationAction actionHandler)
        {
            this.Start(control, transitionType, duration, actionHandler, null);
        }

        /// <summary>
        /// Start animation.
        /// </summary>
        /// <param name="control">Control to use.</param>
        /// <param name="transitionType">TransitionType instance.</param>
        /// <param name="duration">Duration time in milliseconds.</param>
        /// <param name="actionHandler">AnimationAction delegate.</param>
        /// <param name="evaluatorHandler">AnimationFinishedEvaluator delegate.</param>
        protected void Start(Control control, TransitionType transitionType, int duration, AnimationAction actionHandler, AnimationFinishedEvaluator evaluatorHandler)
        {
            this._targetControl = control;
            this.TransitionType = transitionType;
            this._actionHandler = actionHandler;
            this._evaluatorHandler = evaluatorHandler;

            this.Counter = 0;
            this.StartTime = 0;
            this.TargetTime = duration;

            this._timer = DelayedCall.Start(this.DoAnimation, duration);
        }

        /// <summary>
        /// MakeTransition method.
        /// </summary>
        /// <param name="t">Source value t.</param>
        /// <param name="b">Source value b.</param>
        /// <param name="d">Source value d.</param>
        /// <param name="c">Source value c.</param>
        /// <returns>Transition result.</returns>
        protected int MakeTransition(float t, float b, float d, float c)
        {
            switch (this.TransitionType)
            {
                case TransitionType.Linear:
                    return (int)((c * t / d) + b);

                case TransitionType.EaseInQuad:
                    return (int)((c * (t /= d) * t) + b);

                case TransitionType.EaseOutQuad:
                    return (int)((-c * (t = t / d) * (t - 2)) + b);

                case TransitionType.EaseInOutQuad:
                    if ((t /= d / 2) < 1)
                    {
                        return (int)((c / 2 * t * t) + b);
                    }
                    else
                    {
                        return (int)(-c / 2 * ((--t) * (t - 2) - 1) + b);
                    }

                case TransitionType.EaseInCubic:
                    return (int)((c * (t /= d) * t * t) + b);

                case TransitionType.EaseOutCubic:
                    return (int)((c * ((((t = t / d) - 1) * t * t) + 1)) + b);

                case TransitionType.EaseInOutCubic:
                    if ((t /= d / 2) < 1)
                    {
                        return (int)((c / 2 * t * t * t) + b);
                    }
                    else
                    {
                        return (int)(c / 2 * ((t -= 2) * t * t + 2) + b);
                    }

                case TransitionType.EaseInQuart:
                    return (int)((c * (t /= d) * t * t * t) + b);

                case TransitionType.EaseInExpo:
                    if (t == 0)
                    {
                        return (int)b;
                    }
                    else
                    {
                        return (int)(c * Math.Pow(2, (10 * (t / d - 1))) + b);
                    }

                case TransitionType.EaseOutExpo:
                    if (t == d)
                    {
                        return (int)(b + c);
                    }
                    else
                    {
                        return (int)(c * (-Math.Pow(2, -10 * t / d) + 1) + b);
                    }

                default:
                    return 0;
            }
        }

        /// <summary>
        /// OnAnimationCompleted method.
        /// </summary>
        private void OnAnimationCompleted()
        {
            if (this.AnimationCompleted != null)
            {
                this.AnimationCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// DoAnimation method.
        /// </summary>
        private void DoAnimation()
        {
            if (this._evaluatorHandler == null || this._evaluatorHandler.Invoke())
            {
                this.OnAnimationCompleted();
            }
            else
            {
                this._actionHandler.Invoke();
                this.Counter++;
                this._timer.Start();
            }
        }
    }
}
