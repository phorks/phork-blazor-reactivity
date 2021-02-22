namespace Phork.Blazor.Lifecycle
{
    /// <summary>
    /// A RenderElement is an element that is expected to be used in a Blazor component's BuildRenderTree method.
    ///  The element will be created the first time it gets accessed by the code inside the method. 
    ///  It is supposed to be reused in consequent renders. If in any rendering the element is not 
    ///  considered to be active, i.e the code that uses the element is out of reach, the element will be cleaned up.
    /// </summary>
    public interface IRenderElement
    {
        /// <summary>
        /// Indicates if the element has been touched in the current render cycle.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// This method notifies that this element has been active in the current render cycle.
        /// </summary>
        void Touch();

        /// <summary>
        /// This method is supposed to be called when the owning component is (re)rendered. 
        /// If this element has not been active in the respective render cycle, it will be cleaned up
        ///  and true will be returned. It will return false otherwise.
        /// </summary>
        /// <returns>True if the component cleaned up. False otherwise.</returns>
        bool TryCleanUp();
    }
}
