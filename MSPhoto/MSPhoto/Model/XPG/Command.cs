using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace XPG.Extensions
{
    public class Command : DependencyObject, IAttachedObject
    {
        #region EventName

        /// <summary>
        /// Exposes the Dependency Property related to the EventName property
        /// </summary>
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register(
                "EventName",
                typeof(string),
                typeof(Command),
                new PropertyMetadata(DependencyProperty.UnsetValue));

        /// <summary>
        /// Gets or sets the name of the event to bind to the command
        /// </summary>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        #endregion

        #region Binding

        /// <summary>
        /// Exposes the Dependency Property related to the Binding property
        /// </summary>
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.Register(
                "Binding", 
                typeof(Binding), 
                typeof(object), 
                new PropertyMetadata(DependencyProperty.UnsetValue));

        /// <summary>
        /// Gets or Sets the binding to the Command implementing ICommand interface
        /// </summary>
        public Binding Binding
        {
            get { return (Binding)GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        #endregion

        #region ParameterBinding

        /// <summary>
        /// Exposes the Dependency Property related to the ParameterBinding property
        /// </summary>
        public static readonly DependencyProperty ParameterBindingProperty =
            DependencyProperty.Register(
                "ParameterBinding", 
                typeof(object), 
                typeof(object), 
                new PropertyMetadata(DependencyProperty.UnsetValue));

        /// <summary>
        /// Gets or Sets the binding to the CommandParameter sent to the command
        /// </summary>
        public object ParameterBinding
        {
            get { return (object)GetValue(ParameterBindingProperty); }
            set { SetValue(ParameterBindingProperty, value); }
        }

        #endregion

        #region Parameter
        
        /// <summary>
        /// Exposes the Dependency Property related to the Parameter property
        /// </summary>
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(Command), new PropertyMetadata(DependencyProperty.UnsetValue));

        /// <summary>
        /// Gets or Sets the parameter sent to the Command when ParameterBinding is not specified
        /// </summary>
        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        #endregion

        #region IAttachedObject
        
        /// <summary>
        /// Contains the instance of the element associated with this instance
        /// </summary>
        public DependencyObject AssociatedObject { get; private set; }

        /// <summary>
        /// Attaches an instance of a DependencyObject to this instance
        /// </summary>
        /// <param name="dependencyObject">The instance to attach</param>
        public void Attach(DependencyObject dependencyObject)
        {
            this.AssociatedObject = dependencyObject;

            if (!string.IsNullOrEmpty(this.EventName))
            {
                EventWrapper eventHooker = new EventWrapper(this);
                eventHooker.AssociatedObject = dependencyObject;
                EventInfo eventInfo = GetEventInfo(dependencyObject.GetType(), this.EventName);

                if (eventInfo != null)
                {
                    Delegate handler = eventHooker.GetEventHandler(eventInfo);

                    WindowsRuntimeMarshal.AddEventHandler<Delegate>(
                        dlg => (EventRegistrationToken)eventInfo.AddMethod.Invoke(dependencyObject, new object[] { dlg }),
                        etr => eventInfo.RemoveMethod.Invoke(dependencyObject, new object[] { etr }), handler);
                }
            }
        }

        /// <summary>
        /// Detaches the currently associated instance
        /// </summary>
        public void Detach()
        {
            if (this.AssociatedObject != null)
            {
                EventWrapper eventHooker = new EventWrapper(this);
                eventHooker.AssociatedObject = this.AssociatedObject;
                EventInfo eventInfo = GetEventInfo(this.AssociatedObject.GetType(), this.EventName);
                Delegate handler = eventHooker.GetEventHandler(eventInfo);

                WindowsRuntimeMarshal.RemoveEventHandler<Delegate>(
                    etr => eventInfo.RemoveMethod.Invoke(this.AssociatedObject, new object[] { etr }), handler);
            }
        } 

        /// <summary>
        /// Gets the informations about the specified event name of the provided type
        /// </summary>
        /// <param name="type">Type instance to be used to find the event</param>
        /// <param name="eventName">Name of the event to read the info</param>
        /// <returns>Instance of an EventInfo class containing the event information</returns>
        private static EventInfo GetEventInfo(Type type, string eventName)
        {
            EventInfo eventInfo = type.GetTypeInfo().GetDeclaredEvent(eventName);

            if (eventInfo == null)
            {
                Type baseType = type.GetTypeInfo().BaseType;

                if (baseType != null)
                    return GetEventInfo(type.GetTypeInfo().BaseType, eventName);
                else
                    return eventInfo;
            }

            return eventInfo;
        }

        #endregion

        #region Inner Classes
        
        /// <summary>
        /// Represent an inner class able to wrap an event
        /// </summary>
        private sealed class EventWrapper
        {
            /// <summary>
            /// Contains the instance of the element associated with this instance
            /// </summary>
            public DependencyObject AssociatedObject { get; set; }
            /// <summary>
            /// Contains a reference to the command that owns this instance
            /// </summary>
            private Command Binding { get; set; }

            /// <summary>
            /// Initializes the instance of the class
            /// </summary>
            /// <param name="binding">Reference to the command that owns this instance</param>
            public EventWrapper(Command binding)
            {
                this.Binding = binding;
            }

            /// <summary>
            /// Create an instance of a delegate able to handle the provided event
            /// </summary>
            /// <param name="eventInfo">Information about the event to create the delegate</param>
            /// <returns></returns>
            public Delegate GetEventHandler(EventInfo eventInfo)
            {
                Delegate dlg = null;

                if (eventInfo == null)
                    throw new ArgumentNullException("eventInfo");

                if (eventInfo.EventHandlerType == null)
                    throw new ArgumentNullException("eventInfo.EventHandlerType");

                if (dlg == null)
                    dlg = this.GetType().GetTypeInfo().GetDeclaredMethod("OnEventRaised").CreateDelegate(eventInfo.EventHandlerType, this);

                return dlg;
            }

            /// <summary>
            /// Received the notifications when the wrapped event has been raised.
            /// </summary>
            /// <param name="sender">source of the event</param>
            /// <param name="e">arguments of the event</param>
            private void OnEventRaised(object sender, object e)
            {
                BindingEvaluator commandBinding = new BindingEvaluator((FrameworkElement)sender, this.Binding.Binding);
                ICommand command = commandBinding.Value as ICommand;

                object commandParameter;

                if (this.Binding.ParameterBinding is Binding)
                {
                    BindingEvaluator commandParameterBinding = new BindingEvaluator((FrameworkElement)sender, (Binding)this.Binding.ParameterBinding);
                    commandParameter = commandParameterBinding.Value;
                }
                else
                    commandParameter = this.Binding.Parameter;

                if (command != null)
                    command.Execute(commandParameter);
            }
        }

        /// <summary>
        /// Represents a class able to evaluate the value of a binding expression
        /// </summary>
        private sealed class BindingEvaluator : FrameworkElement
        {
            /// <summary>
            /// Instance of the binding to evaluate
            /// </summary>
            private Binding Binding { get; set; }

            #region Value

            /// <summary>
            /// Exposes the Dependency Property related to the Value property
            /// </summary>
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(BindingEvaluator), new PropertyMetadata(DependencyProperty.UnsetValue));

            /// <summary>
            /// Gests the value retrieved from the binding
            /// </summary>
            public object Value
            {
                get
                {
                    return (object)GetValue(ValueProperty);
                }
                private set { SetValue(ValueProperty, value); }
            }

            #endregion

            /// <summary>
            /// Create an instance of the evaluator
            /// </summary>
            /// <param name="element">Instance of element to inherit data context</param>
            /// <param name="binding">Instance of the binding to evaluate</param>
            public BindingEvaluator(FrameworkElement element, Binding binding)
            {
                this.DataContext = element.DataContext;
                this.Binding = binding;
                SetBinding(BindingEvaluator.ValueProperty, this.Binding);
            }
        } 

        #endregion
    }
}
