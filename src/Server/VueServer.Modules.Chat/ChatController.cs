using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Domain;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Chat.Models.Request;
using VueServer.Modules.Chat.Services;
using VueServer.Modules.Core.Controllers.Filters;
using Route = VueServer.Modules.Core.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Modules.Chat
{
    [Authorize(Roles = DomainConstants.Authentication.ROLES_ALL)]
    [Route(ChatConstants.Controller.BasePath)]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public ChatController(IChatService chatService, IStatusCodeFactory<IActionResult> factory)
        {
            _chatService = chatService ?? throw new ArgumentNullException("Chat service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [Route(ChatConstants.Controller.GetActiveConversationUsers)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersFromActiveConversations()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetActiveConversationUsers());
        }

        [Route(ChatConstants.Controller.GetUser)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetUser(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetUsersFromConversation(id));
        }

        [Route(ChatConstants.Controller.StartConversation)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpPost]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.StartConversation(request));
        }

        [Route(ChatConstants.Controller.GetConversation)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetConversation(id));
        }

        [Route(ChatConstants.Controller.GetNewMessageNotifications)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetNewMessageNotifications()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetNewMessageNotifications());
        }

        [Route(ChatConstants.Controller.GetAllConversations)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetAllConversations()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetAllConversations());
        }

        [Route(ChatConstants.Controller.UpdateConversationTitle)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpPost]
        public async Task<IActionResult> UpdateConversationTitle(long conversationId, [FromBody] UpdateConversationTitleRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateConversationTitle(conversationId, request?.Title));
        }

        [Route(ChatConstants.Controller.UpdateUserColor)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpPost]
        public async Task<IActionResult> UpdateUserColor(long conversationId, string userId, [FromBody] UpdateConversationUserColorRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateUserColor(conversationId, userId, request.ColorId));
        }

        [Route(ChatConstants.Controller.DeleteConversation)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id, Feature = ChatConstants.ModuleFeatures.DELETE_CONVERSATION_ID)]
        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(long conversationId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteConversation(conversationId));
        }

        [Route(ChatConstants.Controller.GetMessagesForConversation)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetMessagesForConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessagesForConversation(id));
        }

        [Route(ChatConstants.Controller.GetPaginatedMessagesForConversation)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetPaginatedMessagesForConversation(long conversationId, long msgId)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessagesForConversation(conversationId, msgId));
        }

        [Route(ChatConstants.Controller.DeleteMessage)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id, Feature = ChatConstants.ModuleFeatures.DELETE_MESSAGE_ID)]
        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteMessage(messageId));
        }

        [Route(ChatConstants.Controller.GetMessage)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpGet]
        public async Task<IActionResult> GetMessage(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessage(id));
        }

        [Route(ChatConstants.Controller.AddMessage)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] ChatMessage message)
        {
            return _codeFactory.GetStatusCode(await _chatService.AddMessage(message));
        }

        [Route(ChatConstants.Controller.ReadMessage)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpPut]
        public async Task<IActionResult> ReadMessage(long conversationId, long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessage(conversationId, messageId));
        }

        [Route(ChatConstants.Controller.ReadMessageList)]
        [ModuleAuthFilterFactory(Module = ChatConstants.ModuleAddOn.Id)]
        [HttpPut]
        public async Task<IActionResult> ReadMessageList(long conversationId, [FromBody] long[] messageIds)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessageList(conversationId, messageIds));
        }
    }
}
