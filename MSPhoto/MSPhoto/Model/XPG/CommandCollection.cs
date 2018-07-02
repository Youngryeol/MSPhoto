using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace XPG.Extensions
{
    /// <summary>
    /// Represents a collection of commands
    /// </summary>
    public class CommandCollection : AttachableCollection<Command>
    {
        /// <summary>
        /// Implemented by derived class notifies an item was added
        /// </summary>
        /// <param name="item">Item added</param>
        internal override void ItemAdded(Command item)
        {
            if (base.AssociatedObject != null)
            {
                item.Attach(this.AssociatedObject);
            }
        }

        /// <summary>
        /// Implemented by derived class notifies an item was removed
        /// </summary>
        /// <param name="item">Item removed</param>
        internal override void ItemRemoved(Command item)
        {
            if (base.AssociatedObject != null)
            {
                item.Detach();
            }
        }

        /// <summary>
        /// Notifies a DependencyObject has been attached
        /// </summary>
        protected override void OnAttached()
        {
            foreach (Command binding in this)
                binding.Attach(this.AssociatedObject);
        }

        /// <summary>
        /// Notifies a DependencyObject is detaching
        /// </summary>
        protected override void OnDetaching()
        {
            foreach (Command binding in this)
                binding.Detach();
        }
    }
}