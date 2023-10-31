﻿using System.Collections.Generic;
using System.Windows;

namespace GraphX.Controls.Models
{
    /// <summary>
    /// Default label factory class
    /// </summary>
    /// <typeparam name="TLabel">Type of label to generate. Should be UIElement derived.</typeparam>
    public abstract class DefaultLabelFactory<TLabel> : ILabelFactory<TLabel>
        where TLabel: UIElement, new()
    {
        /// <summary>
        /// Returns newly generated label for parent control. Attachable labels will be auto attached if derived from IAttachableControl<TCtrl>
        /// </summary>
        /// <param name="control">Parent control</param>
        public virtual IEnumerable<TLabel> CreateLabel<TCtrl>(TCtrl control)
        {
            var label = new TLabel();
            var aLabel = label as IAttachableControl<TCtrl>;
            aLabel?.Attach(control);
            return new List<TLabel> {label};
        }
    }

    /// <summary>
    /// Default vertex label factory class
    /// </summary>
    public class DefaultVertexlabelFactory : DefaultLabelFactory<AttachableVertexLabelControl>
    {
        
    }
    /// <summary>
    /// Default edge label factory class
    /// </summary>
    public class DefaultEdgelabelFactory : DefaultLabelFactory<AttachableEdgeLabelControl>
    {

    }
}
