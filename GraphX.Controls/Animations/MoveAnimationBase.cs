using System;
using System.Collections.Generic;
using GraphX.PCL.Common.Exceptions;

namespace GraphX.Controls.Animations
{
    /// <summary>
    /// Base class for move animation.
    /// Logic:
    /// 1. Load all vertex and edge data using AddVertexData and AddEdgeData
    /// 2. Use RunVertexAnimation and RunEdgeAnimation to create and batch-execute all animations
    /// 3. You need to manually trace and execute OnCompleted() method to fire completion event
    /// </summary>
    public abstract class MoveAnimationBase
    {
        /// <summary>
        /// Stores initial vertex data
        /// </summary>
        public Dictionary<IGraphControl, Measure.Point> VertexStorage { get; private set; }
        /// <summary>
        /// Stores initial edges data
        /// </summary>
        public List<IGraphControl> EdgeStorage { get; private set; }

        public MoveAnimationBase()
        {
            VertexStorage = new Dictionary<IGraphControl, Measure.Point>();
            EdgeStorage = new List<IGraphControl>();
            Duration = TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// Optional cleanup that needs to be performed before or after class usage
        /// </summary>
        public abstract void Cleanup();

        /*int _vCount;
        int _eCount;
        int _counter;
        public void PrepareEventListener()
        {
            _vCount = VertexStorage.Count;
            _eCount = EdgeStorage.Count;
            _counter = 0;
            VertexStorage
        }*/

        /// <summary>
        /// Executed before each class usage to clean all existing data left from previous calls
        /// </summary>
        public void CleanupBaseData()
        {
            EdgeStorage.Clear();
            VertexStorage.Clear();
        }

        /// <summary>
        /// Animates the control of a vertex to a given position.
        /// </summary>
        /// <param name="control">Vertex control which should be animated to its new position</param>
        /// <param name="coord">New vertex position coordinates</param>
        public void AddVertexData(IGraphControl control, Measure.Point coord)
        {
            if (double.IsNaN(coord.X) || double.IsNaN(coord.Y))
                throw new GX_InvalidDataException("AddVertexData() -> NaN coordinated has been supplied! Correct coordinates was expected.");
            if (!VertexStorage.ContainsKey(control))
                VertexStorage.Add(control, coord);
            else throw new GX_GeneralException("AddVertexData() -> Same control can't be loaded in animation list twice!");
        }
        /// <summary>
        /// Additional edge animation performed along with vertex animation
        /// </summary>
        /// <param name="control">Edge control</param>
        public void AddEdgeData(IGraphControl control)
        {
            if (!EdgeStorage.Contains(control))
                EdgeStorage.Add(control);
            else throw new GX_GeneralException("AddEdgeData() -> Same control can't be loaded in animation list twice!");
        }
        /// <summary>
        /// Run vertex animations using VertexStorage data
        /// </summary>
        public virtual void RunVertexAnimation() { }
        /// <summary>
        /// Run edge animations using EdgeStorage data
        /// </summary>
        public virtual void RunEdgeAnimation() { }

        /// <summary>
        /// The duration of the animation. Default value is 2.
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Event raised on animation completion
        /// </summary>
        public event EventHandler Completed;
        /// <summary>
        /// Raise on animation completion event
        /// </summary>
        protected void OnCompleted()
        {
            if (Completed != null)
                Completed.Invoke(this, null);
        }
    }
}
