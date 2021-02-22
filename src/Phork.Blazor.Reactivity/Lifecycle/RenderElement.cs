using System;

namespace Phork.Blazor.Lifecycle
{
    /// <summary>
    /// Internal implementation of <see cref="IRenderElement"/>.
    /// </summary>
    internal class RenderElement : IRenderElement
    {
        private bool firstActivation = true;

        private bool _isActive = false;
        public bool IsActive => this._isActive;


        public void Touch()
        {
            if (this._isActive)
            {
                return;
            }

            this._isActive = true;
            this.Activate(this.firstActivation);
            this.firstActivation = false;
        }

        public virtual bool TryCleanUp()
        {
            if (this._isActive)
            {
                this._isActive = false;
                return false;
            }

            (this as IDisposable)?.Dispose();
            return true;
        }

        /// <summary>
        /// This method will be called when the element is accessed for the first time in each 
        ///  render cycle, from the second render onwards.
        /// </summary>
        protected virtual void Activate(bool firstActivation)
        {
        }

    }
}
