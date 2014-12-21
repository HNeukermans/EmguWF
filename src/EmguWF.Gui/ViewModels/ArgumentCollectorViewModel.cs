using System.Activities;
using System.Collections.Generic;
using System.Linq;
using JulMar.Windows.Mvvm;
using System;

namespace EmguWF.ViewModels
{
    /// <summary>
    /// Base class for all arguments
    /// </summary>
    public abstract class ArgumentViewModel : SimpleViewModel
    {
        public DynamicActivityProperty Property { get; set; }
        public string Title { get; set; }
        public abstract object GetValue();
    }

    public abstract class ArgumentViewModel<T> : ArgumentViewModel
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetPropertyValue(ref _value, value); }
        }

        public override object GetValue()
        {
            return _value;
        }
    }

    /// <summary>
    /// Argument view model for text
    /// </summary>
    public sealed class TextArgumentViewModel : ArgumentViewModel<string> {}

    /// <summary>
    /// Argument view model for true/false
    /// </summary>
    public sealed class BooleanArgumentViewModel : ArgumentViewModel<bool> { }

    /// <summary>
    /// Argument view model for date/time
    /// </summary>
    public sealed class DateTimeArgumentViewModel : ArgumentViewModel<DateTime> { }

    /// <summary>
    /// Argument view model for integers
    /// </summary>
    public sealed class IntArgumentViewModel : ArgumentViewModel<int> { }

    /// <summary>
    /// ViewModel to collect arguments for a workflow
    /// </summary>
    public sealed class ArgumentCollectorViewModel : SimpleViewModel
    {
        private readonly DynamicActivity _activity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="activity">Activity to get parameters for</param>
        public ArgumentCollectorViewModel(DynamicActivity activity)
        {
            this._activity = activity;
            Arguments = new List<ArgumentViewModel>(
                _activity.Properties.Select(CreateArgument));
        }

        /// <summary>
        /// Create an argument wrapper for a property
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private ArgumentViewModel CreateArgument(DynamicActivityProperty prop)
        {
            Type innerType = prop.Type.GetGenericArguments().FirstOrDefault() ?? prop.Type;

            if (innerType == typeof(bool))
                return new BooleanArgumentViewModel() {Title = prop.Name, Property = prop };
            if (innerType == typeof(DateTime))
                return new DateTimeArgumentViewModel() { Title = prop.Name, Property = prop };
            if (innerType == typeof(int))
                return new IntArgumentViewModel() { Title = prop.Name, Property = prop };
            
            return new TextArgumentViewModel() { Title = prop.Name + " (" + innerType.Name + ")", Property = prop };
        }

        /// <summary>
        /// List of arguments used
        /// </summary>
        public IList<ArgumentViewModel> Arguments { get; private set; } 

        /// <summary>
        /// True if we need arguments to run this workflow.
        /// </summary>
        public bool HasArguments { get { return Arguments.Any(); }}

        /// <summary>
        /// Name of the wrapped activity
        /// </summary>
        public string ActivityName
        {
            get { return _activity.DisplayName; }
        }

        /// <summary>
        /// This creates the argument dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,object> CollectArguments()
        {
            return Arguments.ToDictionary(a => a.Property.Name, a => a.GetValue());
        }
    }
}
