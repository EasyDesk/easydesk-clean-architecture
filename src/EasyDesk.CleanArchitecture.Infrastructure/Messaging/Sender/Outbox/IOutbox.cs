﻿using EasyDesk.CleanArchitecture.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;

public interface IOutbox
{
    Task StoreMessages(IEnumerable<Message> messages);

    Task PublishMessages(IEnumerable<Guid> messageIds);

    Task Flush();
}