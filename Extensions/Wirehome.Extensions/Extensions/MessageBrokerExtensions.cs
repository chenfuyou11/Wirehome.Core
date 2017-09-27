using Wirehome.Contracts.Messaging;
using Wirehome.Extensions.Messaging.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Extensions
{
    //public static class MessageBrokerExtensions
    //{
    //    public static Task PublishAsync(this IMessageBrokerService messageBroker, IBaseMessage message)
    //    {
    //        if(message.DefaultService == null) throw new ArgumentNullException(nameof(message.DefaultService));
            
    //        return messageBroker.Publish(message.DefaultService.Name, message);
    //    }

    //    public static async Task<T> PublishAsync<T>(this IMessageBrokerService messageBroker, 
    //                                                T message,
    //                                                int millisecondsTimeOut = 2000,
    //                                                CancellationToken cancellationToken = default(CancellationToken)
    //                                                ) where T : IBaseMessage
    //    {
    //        if (message.DefaultService == null) throw new ArgumentNullException(nameof(message.DefaultService));

    //        message.MessageID = Guid.NewGuid();
    //        message.CancellationToken = cancellationToken;

    //        var taskSource = new TaskCompletionSource<T>();
    //        var resultTask = taskSource.Task;

    //        try
    //        {
    //            messageBroker.Subscribe(new MessageSubscription
    //            {
    //                Id = message.MessageID.ToString(),
    //                Topic = message.MessageID.ToString(),
    //                PayloadType = typeof(T).Name,
    //                Callback = result =>
    //                {
    //                    var resultMessage = result.Payload.Content.ToObject<T>();
    //                    if(resultMessage.Error != null)
    //                    {
    //                        taskSource.SetException(resultMessage.Error);
    //                    }
    //                    else
    //                    {
    //                        taskSource.SetResult(resultMessage);
    //                    }
    //                }
    //            });

    //            var publishTask = messageBroker.Publish(message.DefaultService.Name, message);
                
    //            await (new[] { publishTask, resultTask }).WhenAll(millisecondsTimeOut, cancellationToken);
    //        }
    //        catch(Exception)
    //        {
    //            taskSource.SetCanceled();
    //            throw;
    //        }
    //        finally
    //        {
    //            messageBroker.Unsubscribe(message.MessageID.ToString());
    //        }
            
    //        return resultTask.Result;
    //    }        
    //}
}
