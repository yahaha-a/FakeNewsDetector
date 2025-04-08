using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Helpers
{
    /// <summary>
    /// 事件聚合器 - 实现松耦合的事件发布订阅模式
    /// </summary>
    public class EventAggregator
    {
        private static readonly Lazy<EventAggregator> _instance = new Lazy<EventAggregator>(() => new EventAggregator());
        private readonly Dictionary<Type, List<WeakReference>> _eventSubscribers = new Dictionary<Type, List<WeakReference>>();
        private readonly object _lock = new object();

        /// <summary>
        /// 获取事件聚合器单例
        /// </summary>
        public static EventAggregator Instance => _instance.Value;

        private EventAggregator() { }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (!_eventSubscribers.TryGetValue(eventType, out var subscribers))
                {
                    subscribers = new List<WeakReference>();
                    _eventSubscribers[eventType] = subscribers;
                }

                // 检查该处理器是否已订阅，避免重复订阅
                if (!subscribers.Any(s => s.IsAlive && s.Target is Action<TEvent> target && target == handler))
                {
                    subscribers.Add(new WeakReference(handler));
                }

                // 清理无效的引用
                CleanupDeadReferences();
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (_eventSubscribers.TryGetValue(eventType, out var subscribers))
                {
                    // 移除匹配的处理器
                    for (int i = subscribers.Count - 1; i >= 0; i--)
                    {
                        var subscriber = subscribers[i];
                        if (!subscriber.IsAlive || 
                            (subscriber.Target is Action<TEvent> target && target == handler))
                        {
                            subscribers.RemoveAt(i);
                        }
                    }

                    // 如果没有订阅者了，移除整个事件类型
                    if (subscribers.Count == 0)
                    {
                        _eventSubscribers.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="eventToPublish">要发布的事件对象</param>
        public void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            if (eventToPublish == null)
                throw new ArgumentNullException(nameof(eventToPublish));

            List<Action<TEvent>> handlersToNotify = new List<Action<TEvent>>();

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (_eventSubscribers.TryGetValue(eventType, out var subscribers))
                {
                    // 收集有效的处理器
                    for (int i = subscribers.Count - 1; i >= 0; i--)
                    {
                        var subscriber = subscribers[i];
                        if (subscriber.IsAlive)
                        {
                            if (subscriber.Target is Action<TEvent> handler)
                            {
                                handlersToNotify.Add(handler);
                            }
                        }
                        else
                        {
                            subscribers.RemoveAt(i);
                        }
                    }

                    // 如果没有订阅者了，移除整个事件类型
                    if (subscribers.Count == 0)
                    {
                        _eventSubscribers.Remove(eventType);
                    }
                }
            }

            // 在锁外调用处理器，避免死锁
            foreach (var handler in handlersToNotify)
            {
                try
                {
                    handler(eventToPublish);
                }
                catch (Exception ex)
                {
                    // 这里可以记录错误，但不应该让一个处理器的异常影响其他处理器
                    System.Diagnostics.Debug.WriteLine($"事件处理异常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="eventToPublish">要发布的事件对象</param>
        public async Task PublishAsync<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            if (eventToPublish == null)
                throw new ArgumentNullException(nameof(eventToPublish));

            // 在线程池中发布事件
            await Task.Run(() => Publish(eventToPublish));
        }

        /// <summary>
        /// 清理失效的引用
        /// </summary>
        private void CleanupDeadReferences()
        {
            foreach (var eventType in _eventSubscribers.Keys.ToList())
            {
                var subscribers = _eventSubscribers[eventType];
                
                // 移除无效引用
                for (int i = subscribers.Count - 1; i >= 0; i--)
                {
                    if (!subscribers[i].IsAlive)
                    {
                        subscribers.RemoveAt(i);
                    }
                }

                // 如果没有订阅者了，移除整个事件类型
                if (subscribers.Count == 0)
                {
                    _eventSubscribers.Remove(eventType);
                }
            }
        }
    }
} 