using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;

namespace XPG.Extensions
{
    /// <summary>
    /// Describes an object that can be associated to another DependencyObject
    /// </summary>
    public interface IAttachedObject
    {
        /// <summary>
        /// Attaches a DependencyObject to this instance
        /// </summary>
        /// <param name="dependencyObject">The dependencyObject to attach</param>
        void Attach(DependencyObject dependencyObject);
        /// <summary>
        /// Detaches the currently associated DependencyObject
        /// </summary>
        void Detach();
        /// <summary>
        /// Exposes the instance of the associated DependencyObject
        /// </summary>
        DependencyObject AssociatedObject { get; }
    }
}
