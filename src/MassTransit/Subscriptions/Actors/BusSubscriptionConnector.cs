// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Subscriptions.Actors
{
	using System;
	using System.Collections.Generic;
	using Messages;
	using log4net;

	public class BusSubscriptionConnector :
		BusSubscriptionEventObserver
	{
		static readonly ILog _log = LogManager.GetLogger(typeof (BusSubscriptionConnector));
		readonly IServiceBus _bus;
		readonly EndpointSubscriptionConnectorCache _cache;
		readonly Dictionary<Guid, UnsubscribeAction> _connectionCache;

		public BusSubscriptionConnector(IServiceBus bus)
		{
			_bus = bus;
			_cache = new EndpointSubscriptionConnectorCache(bus);
			_connectionCache = new Dictionary<Guid, UnsubscribeAction>();
		}

		public void OnSubscriptionAdded(SubscriptionAddedMessage message)
		{
			_connectionCache[message.SubscriptionId] = _cache.Connect(message.MessageName, _bus.Endpoint.Address.Uri,
				message.CorrelationId);

			if (_log.IsDebugEnabled)
				_log.DebugFormat("SubscriptionAdded: {0}, {1}", message.MessageName, message.SubscriptionId);
		}

		public void OnSubscriptionRemoved(SubscriptionRemovedMessage message)
		{
			UnsubscribeAction unsubscribe;
			if (_connectionCache.TryGetValue(message.SubscriptionId, out unsubscribe))
			{
				unsubscribe();
				_connectionCache.Remove(message.SubscriptionId);

				if (_log.IsDebugEnabled)
					_log.DebugFormat("SubscriptionRemoved: {0}, {1}", message.MessageName, message.SubscriptionId);
			}
		}
	}
}