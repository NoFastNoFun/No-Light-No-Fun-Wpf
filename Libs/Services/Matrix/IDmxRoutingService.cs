using Core.Messages;

namespace Services.Matrix
{
    public interface IDmxRoutingService
    {
        void RouteUpdate(UpdateMessage packet);
    }
} 