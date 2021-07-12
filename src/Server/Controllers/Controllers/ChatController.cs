using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.Status;
using VueServer.Models.Chat;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using AddOns = VueServer.Domain.DomainConstants.Models.ModuleAddOns;
using Features = VueServer.Domain.DomainConstants.Models.ModuleFeatures;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Chat.Controller)]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public ChatController(IChatService chatService, IStatusCodeFactory<IActionResult> factory)
        {
            _chatService = chatService ?? throw new ArgumentNullException("Chat service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [Route(Route.Chat.StartConversation)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.StartConversation(request));
        }

        [Route(Route.Chat.GetConversation)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetConversation(id));
        }

        [Route(Route.Chat.GetNewMessageNotifications)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetNewMessageNotifications()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetNewMessageNotifications());
        }

        [Route(Route.Chat.GetAllConversations)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetAllConversations()

        {
            return _codeFactory.GetStatusCode(await _chatService.GetAllConversations());
        }

        [Route(Route.Chat.UpdateConversationTitle)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> UpdateConversationTitle(long conversationId, [FromBody] UpdateConversationTitleRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateConversationTitle(conversationId, request?.Title));
        }

        [Route(Route.Chat.UpdateUserColor)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> UpdateUserColor(long conversationId, string userId, [FromBody] UpdateConversationUserColorRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateUserColor(conversationId, userId, request.ColorId));
        }

        [Route(Route.Chat.DeleteConversation)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id, Feature = Features.Chat.DELETE_CONVERSATION_ID)]
        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(long conversationId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteConversation(conversationId));
        }

        [Route(Route.Chat.GetMessagesForConversation)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetMessagesForConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessagesForConversation(id));
        }

        [Route(Route.Chat.DeleteMessage)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id, Feature = Features.Chat.DELETE_MESSAGE_ID)]
        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteMessage(messageId));
        }

        [Route(Route.Chat.GetMessage)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetMessage(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessage(id));
        }

        [Route(Route.Chat.AddMessage)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] ChatMessage message)
        {
            return _codeFactory.GetStatusCode(await _chatService.AddMessage(message));
        }

        [Route(Route.Chat.ReadMessage)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPut]
        public async Task<IActionResult> ReadMessage(long conversationId, long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessage(conversationId, messageId));
        }

        [Route(Route.Chat.ReadMessageList)]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPut]
        public async Task<IActionResult> ReadMessageList(long conversationId, [FromBody] long[] messageIds)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessageList(conversationId, messageIds));
        }
    }
}
