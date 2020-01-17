﻿using MediatR;
using Project.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Api.Applications.DomainEventsHandlers
{
    public class ProjectJoinedDomainEventsHandler : INotificationHandler<ProjectJoinedEvent>
    {
        public Task Handle(ProjectJoinedEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
