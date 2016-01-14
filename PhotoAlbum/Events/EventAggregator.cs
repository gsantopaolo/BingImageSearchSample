using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;


namespace PhotoAlbum.Events
{
    /// <summary>
    /// Specifies on which thread a <see cref="PubSubEvent{TPayload}"/> subscriber will be called.
    /// </summary>
    public enum ThreadOption
    {
        /// <summary>
        /// The call is done on the same thread on which the <see cref="PubSubEvent{TPayload}"/> was published.
        /// </summary>
        PublisherThread,

        /// <summary>
        /// The call is done on the UI thread.
        /// </summary>
        UIThread,

        /// <summary>
        /// The call is done asynchronously on a background thread.
        /// </summary>
        BackgroundThread
    }

    /// <summary>
    /// Represents a reference to a <see cref="Delegate"/>.
    /// </summary>
    public interface IDelegateReference
    {
        /// <summary>
        /// Gets the referenced <see cref="Delegate" /> object.
        /// </summary>
        /// <value>A <see cref="Delegate"/> instance if the target is valid; otherwise <see langword="null"/>.</value>
        Delegate Target { get; }
    }

    /// <summary>
    /// Represents a reference to a <see cref="Delegate"/> that may contain a
    /// <see cref="WeakReference"/> to the target. This class is used
    /// internally by the Prism Library.
    /// </summary>
    public class DelegateReference : IDelegateReference
    {
        private readonly Delegate _delegate;
        private readonly WeakReference _weakReference;
        private readonly MethodInfo _method;
        private readonly Type _delegateType;

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateReference"/>.
        /// </summary>
        /// <param name="delegate">The original <see cref="Delegate"/> to create a reference for.</param>
        /// <param name="keepReferenceAlive">If <see langword="false" /> the class will create a weak reference to the delegate, allowing it to be garbage collected. Otherwise it will keep a strong reference to the target.</param>
        /// <exception cref="ArgumentNullException">If the passed <paramref name="delegate"/> is not assignable to <see cref="Delegate"/>.</exception>
        public DelegateReference(Delegate @delegate, bool keepReferenceAlive)
        {
            if (@delegate == null)
                throw new ArgumentNullException("delegate");

            if (keepReferenceAlive)
            {
                this._delegate = @delegate;
            }
            else
            {
                _weakReference = new WeakReference(@delegate.Target);
                _method = @delegate.GetMethodInfo();
                _delegateType = @delegate.GetType();
            }
        }

        /// <summary>
        /// Gets the <see cref="Delegate" /> (the target) referenced by the current <see cref="DelegateReference"/> object.
        /// </summary>
        /// <value><see langword="null"/> if the object referenced by the current <see cref="DelegateReference"/> object has been garbage collected; otherwise, a reference to the <see cref="Delegate"/> referenced by the current <see cref="DelegateReference"/> object.</value>
        public Delegate Target
        {
            get
            {
                if (_delegate != null)
                {
                    return _delegate;
                }
                else
                {
                    return TryGetDelegate();
                }
            }
        }

        private Delegate TryGetDelegate()
        {
            if (_method.IsStatic)
            {
                return _method.CreateDelegate(_delegateType, null);
            }
            object target = _weakReference.Target;
            if (target != null)
            {
                return _method.CreateDelegate(_delegateType, target);
            }
            return null;
        }
    }

    /// <summary>
    /// Defines an interface to get instances of an event type.
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Gets an instance of an event type.
        /// </summary>
        /// <typeparam name="TEventType">The type of event to get.</typeparam>
        /// <returns>An instance of an event object of type <typeparamref name="TEventType"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        TEventType GetEvent<TEventType>() where TEventType : EventBase, new();
    }

    /// <summary>
    /// Implements <see cref="IEventAggregator"/>.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, EventBase> events = new Dictionary<Type, EventBase>();

        static EventAggregator()
        {
            ServiceContainer.Instance.AddService<IEventAggregator>(new EventAggregator());
        }

        // Captures the sync context for the UI thread when constructed on the UI thread 
        // in a platform agnositc way so it can be used for UI thread dispatching
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        /// <summary>
        /// Gets the single instance of the event managed by this EventAggregator. Multiple calls to this method with the same <typeparamref name="TEventType"/> returns the same event instance.
        /// </summary>
        /// <typeparam name="TEventType">The type of event to get. This must inherit from <see cref="EventBase"/>.</typeparam>
        /// <returns>A singleton instance of an event object of type <typeparamref name="TEventType"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
        {
            lock (events)
            {
                EventBase existingEvent = null;

                if (!events.TryGetValue(typeof(TEventType), out existingEvent))
                {
                    TEventType newEvent = new TEventType();
                    newEvent.SynchronizationContext = syncContext;
                    events[typeof(TEventType)] = newEvent;

                    return newEvent;
                }
                else
                {
                    return (TEventType)existingEvent;
                }
            }
        }

        public static TEventType Istance<TEventType>()
            where TEventType : EventBase, new()
        {
            return ServiceContainer.Instance.GetService<IEventAggregator>().GetEvent<TEventType>();
        }
    }

    /// <summary>
    /// Defines a class that manages publication and subscription to events.
    /// </summary>
    /// <typeparam name="TPayload">The type of message that will be passed to the subscribers.</typeparam>
    public class PubSubEvent<TPayload> : EventBase
    {
        /// <summary>
        /// Subscribes a delegate to an event that will be published on the <see cref="ThreadOption.PublisherThread"/>.
        /// <see cref="PubSubEvent{TPayload}"/> will maintain a <see cref="WeakReference"/> to the target of the supplied <paramref name="action"/> delegate.
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is published.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        /// <remarks>
        /// The PubSubEvent collection is thread-safe.
        /// </remarks>
        public SubscriptionToken Subscribe(Action<TPayload> action)
        {
            return Subscribe(action, ThreadOption.PublisherThread);
        }

        /// <summary>
        /// Subscribes a delegate to an event.
        /// PubSubEvent will maintain a <see cref="WeakReference"/> to the Target of the supplied <paramref name="action"/> delegate.
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is raised.</param>
        /// <param name="threadOption">Specifies on which thread to receive the delegate callback.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        /// <remarks>
        /// The PubSubEvent collection is thread-safe.
        /// </remarks>
        public SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption)
        {
            return Subscribe(action, threadOption, false);
        }

        /// <summary>
        /// Subscribes a delegate to an event that will be published on the <see cref="ThreadOption.PublisherThread"/>.
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is published.</param>
        /// <param name="keepSubscriberReferenceAlive">When <see langword="true"/>, the <see cref="PubSubEvent{TPayload}"/> keeps a reference to the subscriber so it does not get garbage collected.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        /// <remarks>
        /// If <paramref name="keepSubscriberReferenceAlive"/> is set to <see langword="false" />, <see cref="PubSubEvent{TPayload}"/> will maintain a <see cref="WeakReference"/> to the Target of the supplied <paramref name="action"/> delegate.
        /// If not using a WeakReference (<paramref name="keepSubscriberReferenceAlive"/> is <see langword="true" />), the user must explicitly call Unsubscribe for the event when disposing the subscriber in order to avoid memory leaks or unexpected behavior.
        /// <para/>
        /// The PubSubEvent collection is thread-safe.
        /// </remarks>
        public SubscriptionToken Subscribe(Action<TPayload> action, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive);
        }

        /// <summary>
        /// Subscribes a delegate to an event.
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is published.</param>
        /// <param name="threadOption">Specifies on which thread to receive the delegate callback.</param>
        /// <param name="keepSubscriberReferenceAlive">When <see langword="true"/>, the <see cref="PubSubEvent{TPayload}"/> keeps a reference to the subscriber so it does not get garbage collected.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        /// <remarks>
        /// If <paramref name="keepSubscriberReferenceAlive"/> is set to <see langword="false" />, <see cref="PubSubEvent{TPayload}"/> will maintain a <see cref="WeakReference"/> to the Target of the supplied <paramref name="action"/> delegate.
        /// If not using a WeakReference (<paramref name="keepSubscriberReferenceAlive"/> is <see langword="true" />), the user must explicitly call Unsubscribe for the event when disposing the subscriber in order to avoid memory leaks or unexpected behavior.
        /// <para/>
        /// The PubSubEvent collection is thread-safe.
        /// </remarks>
        public SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, threadOption, keepSubscriberReferenceAlive, null);
        }

        /// <summary>
        /// Subscribes a delegate to an event.
        /// </summary>
        /// <param name="action">The delegate that gets executed when the event is published.</param>
        /// <param name="threadOption">Specifies on which thread to receive the delegate callback.</param>
        /// <param name="keepSubscriberReferenceAlive">When <see langword="true"/>, the <see cref="PubSubEvent{TPayload}"/> keeps a reference to the subscriber so it does not get garbage collected.</param>
        /// <param name="filter">Filter to evaluate if the subscriber should receive the event.</param>
        /// <returns>A <see cref="SubscriptionToken"/> that uniquely identifies the added subscription.</returns>
        /// <remarks>
        /// If <paramref name="keepSubscriberReferenceAlive"/> is set to <see langword="false" />, <see cref="PubSubEvent{TPayload}"/> will maintain a <see cref="WeakReference"/> to the Target of the supplied <paramref name="action"/> delegate.
        /// If not using a WeakReference (<paramref name="keepSubscriberReferenceAlive"/> is <see langword="true" />), the user must explicitly call Unsubscribe for the event when disposing the subscriber in order to avoid memory leaks or unexpected behavior.
        /// 
        /// The PubSubEvent collection is thread-safe.
        /// </remarks>
        public virtual SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption, bool keepSubscriberReferenceAlive, Predicate<TPayload> filter)
        {
            IDelegateReference actionReference = new DelegateReference(action, keepSubscriberReferenceAlive);
            IDelegateReference filterReference;
            if (filter != null)
            {
                filterReference = new DelegateReference(filter, keepSubscriberReferenceAlive);
            }
            else
            {
                filterReference = new DelegateReference(new Predicate<TPayload>(delegate { return true; }), true);
            }
            EventSubscription<TPayload> subscription;
            switch (threadOption)
            {
                case ThreadOption.PublisherThread:
                    subscription = new EventSubscription<TPayload>(actionReference, filterReference);
                    break;
                case ThreadOption.BackgroundThread:
                    subscription = new BackgroundEventSubscription<TPayload>(actionReference, filterReference);
                    break;
                case ThreadOption.UIThread:
                    if (SynchronizationContext == null) throw new InvalidOperationException("Resources.EventAggregatorNotConstructedOnUIThread");
                    subscription = new DispatcherEventSubscription<TPayload>(actionReference, filterReference, SynchronizationContext);
                    break;
                default:
                    subscription = new EventSubscription<TPayload>(actionReference, filterReference);
                    break;
            }


            return base.InternalSubscribe(subscription);
        }

        ///<summary>
        /// Extends <see cref="EventSubscription{TPayload}"/> to invoke the <see cref="EventSubscription{TPayload}.Action"/> delegate
        /// in a specific <see cref="SynchronizationContext"/>.
        ///</summary>
        /// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
        public class DispatcherEventSubscription<TPayload> : EventSubscription<TPayload>
        {
            private readonly SynchronizationContext syncContext;

            ///<summary>
            /// Creates a new instance of <see cref="BackgroundEventSubscription{TPayload}"/>.
            ///</summary>
            ///<param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
            ///<param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
            ///<param name="context">The synchronization context to use for UI thread dispatching.</param>
            ///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
            ///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
            ///or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
            public DispatcherEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference, SynchronizationContext context)
                : base(actionReference, filterReference)
            {
                syncContext = context;
            }

            /// <summary>
            /// Invokes the specified <see cref="System.Action{TPayload}"/> asynchronously in the specified <see cref="SynchronizationContext"/>.
            /// </summary>
            /// <param name="action">The action to execute.</param>
            /// <param name="argument">The payload to pass <paramref name="action"/> while invoking it.</param>
            public override void InvokeAction(Action<TPayload> action, TPayload argument)
            {
                syncContext.Post((o) => action((TPayload)o), argument);
            }
        }

        /// <summary>
        /// Extends <see cref="EventSubscription{TPayload}"/> to invoke the <see cref="EventSubscription{TPayload}.Action"/> delegate in a background thread.
        /// </summary>
        /// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
        public class BackgroundEventSubscription<TPayload> : EventSubscription<TPayload>
        {
            /// <summary>
            /// Creates a new instance of <see cref="BackgroundEventSubscription{TPayload}"/>.
            /// </summary>
            /// <param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
            /// <param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
            /// <exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
            /// <exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
            /// or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
            public BackgroundEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
                : base(actionReference, filterReference)
            {
            }

            /// <summary>
            /// Invokes the specified <see cref="System.Action{TPayload}"/> in an asynchronous thread by using a <see cref="ThreadPool"/>.
            /// </summary>
            /// <param name="action">The action to execute.</param>
            /// <param name="argument">The payload to pass <paramref name="action"/> while invoking it.</param>
            public override void InvokeAction(Action<TPayload> action, TPayload argument)
            {
                //ThreadPool.QueueUserWorkItem( (o) => action(argument) );
                System.Threading.Tasks.Task.Run(() => action(argument));
            }
        }

        /// <summary>
        /// Publishes the <see cref="PubSubEvent{TPayload}"/>.
        /// </summary>
        /// <param name="payload">Message to pass to the subscribers.</param>
        public virtual void Publish(TPayload payload)
        {
            base.InternalPublish(payload);
        }

        /// <summary>
        /// Removes the first subscriber matching <see cref="Action{TPayload}"/> from the subscribers' list.
        /// </summary>
        /// <param name="subscriber">The <see cref="Action{TPayload}"/> used when subscribing to the event.</param>
        public virtual void Unsubscribe(Action<TPayload> subscriber)
        {
            lock (Subscriptions)
            {
                IEventSubscription eventSubscription = Subscriptions.Cast<EventSubscription<TPayload>>().FirstOrDefault(evt => evt.Action == subscriber);
                if (eventSubscription != null)
                {
                    Subscriptions.Remove(eventSubscription);
                }
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if there is a subscriber matching <see cref="Action{TPayload}"/>.
        /// </summary>
        /// <param name="subscriber">The <see cref="Action{TPayload}"/> used when subscribing to the event.</param>
        /// <returns><see langword="true"/> if there is an <see cref="Action{TPayload}"/> that matches; otherwise <see langword="false"/>.</returns>
        public virtual bool Contains(Action<TPayload> subscriber)
        {
            IEventSubscription eventSubscription;
            lock (Subscriptions)
            {
                eventSubscription = Subscriptions.Cast<EventSubscription<TPayload>>().FirstOrDefault(evt => evt.Action == subscriber);
            }
            return eventSubscription != null;
        }

    }

    /// <summary>
    /// Provides a way to retrieve a <see cref="Delegate"/> to execute an action depending
    /// on the value of a second filter predicate that returns true if the action should execute.
    /// </summary>
    /// <typeparam name="TPayload">The type to use for the generic <see cref="System.Action{TPayload}"/> and <see cref="Predicate{TPayload}"/> types.</typeparam>
    public class EventSubscription<TPayload> : IEventSubscription
    {
        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        ///<summary>
        /// Creates a new instance of <see cref="EventSubscription{TPayload}"/>.
        ///</summary>
        ///<param name="actionReference">A reference to a delegate of type <see cref="System.Action{TPayload}"/>.</param>
        ///<param name="filterReference">A reference to a delegate of type <see cref="Predicate{TPayload}"/>.</param>
        ///<exception cref="ArgumentNullException">When <paramref name="actionReference"/> or <see paramref="filterReference"/> are <see langword="null" />.</exception>
        ///<exception cref="ArgumentException">When the target of <paramref name="actionReference"/> is not of type <see cref="System.Action{TPayload}"/>,
        ///or the target of <paramref name="filterReference"/> is not of type <see cref="Predicate{TPayload}"/>.</exception>
        public EventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
        {
            if (actionReference == null)
                throw new ArgumentNullException("actionReference");
            if (!(actionReference.Target is Action<TPayload>))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Resources.InvalidDelegateRerefenceTypeException", typeof(Action<TPayload>).FullName), "actionReference");

            if (filterReference == null)
                throw new ArgumentNullException("filterReference");
            if (!(filterReference.Target is Predicate<TPayload>))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Resources.InvalidDelegateRerefenceTypeException", typeof(Predicate<TPayload>).FullName), "filterReference");

            _actionReference = actionReference;
            _filterReference = filterReference;
        }

        /// <summary>
        /// Gets the target <see cref="System.Action{T}"/> that is referenced by the <see cref="IDelegateReference"/>.
        /// </summary>
        /// <value>An <see cref="System.Action{T}"/> or <see langword="null" /> if the referenced target is not alive.</value>
        public Action<TPayload> Action
        {
            get { return (Action<TPayload>)_actionReference.Target; }
        }

        /// <summary>
        /// Gets the target <see cref="Predicate{T}"/> that is referenced by the <see cref="IDelegateReference"/>.
        /// </summary>
        /// <value>An <see cref="Predicate{T}"/> or <see langword="null" /> if the referenced target is not alive.</value>
        public Predicate<TPayload> Filter
        {
            get { return (Predicate<TPayload>)_filterReference.Target; }
        }

        /// <summary>
        /// Gets or sets a <see cref="SubscriptionToken"/> that identifies this <see cref="IEventSubscription"/>.
        /// </summary>
        /// <value>A token that identifies this <see cref="IEventSubscription"/>.</value>
        public SubscriptionToken SubscriptionToken { get; set; }

        /// <summary>
        /// Gets the execution strategy to publish this event.
        /// </summary>
        /// <returns>An <see cref="System.Action{T}"/> with the execution strategy, or <see langword="null" /> if the <see cref="IEventSubscription"/> is no longer valid.</returns>
        /// <remarks>
        /// If <see cref="Action"/> or <see cref="Filter"/> are no longer valid because they were
        /// garbage collected, this method will return <see langword="null" />.
        /// Otherwise it will return a delegate that evaluates the <see cref="Filter"/> and if it
        /// returns <see langword="true" /> will then call <see cref="InvokeAction"/>. The returned
        /// delegate holds hard references to the <see cref="Action"/> and <see cref="Filter"/> target
        /// <see cref="Delegate">delegates</see>. As long as the returned delegate is not garbage collected,
        /// the <see cref="Action"/> and <see cref="Filter"/> references delegates won't get collected either.
        /// </remarks>
        public virtual Action<object[]> GetExecutionStrategy()
        {
            Action<TPayload> action = this.Action;
            Predicate<TPayload> filter = this.Filter;
            if (action != null && filter != null)
            {
                return arguments =>
                {
                    TPayload argument = default(TPayload);
                    if (arguments != null && arguments.Length > 0 && arguments[0] != null)
                    {
                        argument = (TPayload)arguments[0];
                    }
                    if (filter(argument))
                    {
                        InvokeAction(action, argument);
                    }
                };
            }
            return null;
        }

        /// <summary>
        /// Invokes the specified <see cref="System.Action{TPayload}"/> synchronously when not overridden.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="argument">The payload to pass <paramref name="action"/> while invoking it.</param>
        /// <exception cref="ArgumentNullException">An <see cref="ArgumentNullException"/> is thrown if <paramref name="action"/> is null.</exception>
        public virtual void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            if (action == null) throw new System.ArgumentNullException("action");

            action(argument);
        }
    }

    /// <summary>
    /// Provides a singleton container for storing and retrieving services by <see cref="Type"/>.
    /// </summary>
    public class ServiceContainer
    {

        #region Declarations

        readonly Object _lockObject = new Object();
        readonly Dictionary<Type, object> _services = new Dictionary<Type, Object>();

        /// <summary>
        /// Provides access to the ServiceLocator singleton.
        /// </summary>
        public static readonly ServiceContainer Instance = new ServiceContainer();

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <c>ServiceLocator</c> has any stored services.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public Boolean IsEmpty
        {
            get
            {
                lock (_lockObject)
                {
                    return _services.Count == 0;
                }
            }
        }

        #endregion

        #region Private Constructor

        ServiceContainer() { }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a service implementation to the repository.  If the service already exists, it will replace it.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of service to retrieve.</typeparam>
        /// <param name="service">Instance of the service to add.</param>
        public void AddService<T>(T service)
            where T : class
        {
            lock (_lockObject)
            {
                _services[typeof(T)] = service;
            }
        }

        /// <summary>
        /// Gets an instance of the requested service if available.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of service to retrieve.</typeparam>
        /// <returns>The requested instance of the service or null.</returns>
        public T GetService<T>()
            where T : class
        {
            object service;
            lock (_lockObject)
            {
                _services.TryGetValue(typeof(T), out service);

            }
            return service as T;
        }

        #endregion
    }

    ///<summary>
    /// Defines a base class to publish and subscribe to events.
    ///</summary>
    public abstract class EventBase
    {
        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();

        /// <summary>
        /// Allows the SynchronizationContext to be set by the EventAggregator for UI Thread Dispatching
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// Gets the list of current subscriptions.
        /// </summary>
        /// <value>The current subscribers.</value>
        protected ICollection<IEventSubscription> Subscriptions
        {
            get { return _subscriptions; }
        }

        /// <summary>
        /// Adds the specified <see cref="IEventSubscription"/> to the subscribers' collection.
        /// </summary>
        /// <param name="eventSubscription">The subscriber.</param>
        /// <returns>The <see cref="SubscriptionToken"/> that uniquely identifies every subscriber.</returns>
        /// <remarks>
        /// Adds the subscription to the internal list and assigns it a new <see cref="SubscriptionToken"/>.
        /// </remarks>
        protected virtual SubscriptionToken InternalSubscribe(IEventSubscription eventSubscription)
        {
            if (eventSubscription == null) throw new System.ArgumentNullException("eventSubscription");

            eventSubscription.SubscriptionToken = new SubscriptionToken(Unsubscribe);

            lock (Subscriptions)
            {
                Subscriptions.Add(eventSubscription);
            }
            return eventSubscription.SubscriptionToken;
        }

        /// <summary>
        /// Calls all the execution strategies exposed by the list of <see cref="IEventSubscription"/>.
        /// </summary>
        /// <param name="arguments">The arguments that will be passed to the listeners.</param>
        /// <remarks>Before executing the strategies, this class will prune all the subscribers from the
        /// list that return a <see langword="null" /> <see cref="Action{T}"/> when calling the
        /// <see cref="IEventSubscription.GetExecutionStrategy"/> method.</remarks>
        protected virtual void InternalPublish(params object[] arguments)
        {
            List<Action<object[]>> executionStrategies = PruneAndReturnStrategies();
            foreach (var executionStrategy in executionStrategies)
            {
                executionStrategy(arguments);
            }
        }

        /// <summary>
        /// Removes the subscriber matching the <see cref="SubscriptionToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SubscriptionToken"/> returned by <see cref="EventBase"/> while subscribing to the event.</param>
        public virtual void Unsubscribe(SubscriptionToken token)
        {
            lock (Subscriptions)
            {
                IEventSubscription subscription = Subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);
                if (subscription != null)
                {
                    Subscriptions.Remove(subscription);
                }
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if there is a subscriber matching <see cref="SubscriptionToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SubscriptionToken"/> returned by <see cref="EventBase"/> while subscribing to the event.</param>
        /// <returns><see langword="true"/> if there is a <see cref="SubscriptionToken"/> that matches; otherwise <see langword="false"/>.</returns>
        public virtual bool Contains(SubscriptionToken token)
        {
            lock (Subscriptions)
            {
                IEventSubscription subscription = Subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);
                return subscription != null;
            }
        }

        private List<Action<object[]>> PruneAndReturnStrategies()
        {
            List<Action<object[]>> returnList = new List<Action<object[]>>();

            lock (Subscriptions)
            {
                for (var i = Subscriptions.Count - 1; i >= 0; i--)
                {
                    Action<object[]> listItem =
                        _subscriptions[i].GetExecutionStrategy();

                    if (listItem == null)
                    {
                        // Prune from main list. Log?
                        _subscriptions.RemoveAt(i);
                    }
                    else
                    {
                        returnList.Add(listItem);
                    }
                }
            }

            return returnList;
        }
    }

    ///<summary>
    /// Defines a contract for an event subscription to be used by <see cref="EventBase"/>.
    ///</summary>
    public interface IEventSubscription
    {
        /// <summary>
        /// Gets or sets a <see cref="SubscriptionToken"/> that identifies this <see cref="IEventSubscription"/>.
        /// </summary>
        /// <value>A token that identifies this <see cref="IEventSubscription"/>.</value>
        SubscriptionToken SubscriptionToken { get; set; }

        /// <summary>
        /// Gets the execution strategy to publish this event.
        /// </summary>
        /// <returns>An <see cref="Action{T}"/> with the execution strategy, or <see langword="null" /> if the <see cref="IEventSubscription"/> is no longer valid.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Action<object[]> GetExecutionStrategy();
    }

    /// <summary>
    /// Subscription token returned from <see cref="EventBase"/> on subscribe.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Should never have a need for a finalizer, hence no need for Dispole(bool)")]
    public class SubscriptionToken : IEquatable<SubscriptionToken>, IDisposable
    {
        private readonly Guid _token;
        private Action<SubscriptionToken> _unsubscribeAction;

        /// <summary>
        /// Initializes a new instance of <see cref="SubscriptionToken"/>.
        /// </summary>
        public SubscriptionToken(Action<SubscriptionToken> unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
            _token = Guid.NewGuid();
        }

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///<returns>
        ///<see langword="true"/> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false"/>.
        ///</returns>
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(SubscriptionToken other)
        {
            if (other == null) return false;
            return Equals(_token, other._token);
        }

        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        ///</summary>
        ///<returns>
        ///true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        ///</returns>
        ///<param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        ///<exception cref="T:System.NullReferenceException">The <paramref name="obj" /> parameter is null.</exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as SubscriptionToken);
        }

        ///<summary>
        ///Serves as a hash function for a particular type. 
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object" />.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return _token.GetHashCode();
        }

        /// <summary>
        /// Disposes the SubscriptionToken, removing the subscription from the corresponding <see cref="EventBase"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Should never have need for a finalizer, hence no need for Dispose(bool).")]
        public virtual void Dispose()
        {
            // While the SubsctiptionToken class implements IDisposable, in the case of weak subscriptions 
            // (i.e. keepSubscriberReferenceAlive set to false in the Subscribe method) it's not necessary to unsubscribe,
            // as no resources should be kept alive by the event subscription. 
            // In such cases, if a warning is issued, it could be suppressed.

            if (this._unsubscribeAction != null)
            {
                this._unsubscribeAction(this);
                this._unsubscribeAction = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}