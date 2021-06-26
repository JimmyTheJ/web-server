using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.StatusFactory;
using VueServer.Models.Chat;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using AddOns = VueServer.Domain.Constants.Models.ModuleAddOns;
using Features = VueServer.Domain.Constants.Models.ModuleFeatures;

namespace VueServer.Controllers
{
    [Route("/api/chat")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public ChatController(IChatService chatService, IStatusCodeFactory<IActionResult> factory)
        {
            _chatService = chatService ?? throw new ArgumentNullException("Chat service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [Route("conversation/start")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.StartConversation(request));
        }

        [Route("conversation/get/{id}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetConversation(id));
        }

        [Route("conversation/notifications/get-all")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetNewMessageNotifications()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetNewMessageNotifications());
        }

        [Route("conversation/get-all")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetAllConversations()

        {
            return _codeFactory.GetStatusCode(await _chatService.GetAllConversations());
        }

        [Route("conversation/update-title/{conversationId}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> UpdateConversationTitle(long conversationId, [FromBody] UpdateConversationTitleRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateConversationTitle(conversationId, request?.Title));
        }

        [Route("conversation/update-conversation-color/{conversationId}/{userId}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> UpdateUserColor(long conversationId, string userId, [FromBody] UpdateConversationUserColorRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateUserColor(conversationId, userId, request.ColorId));
        }

        [Route("conversation/delete/{conversationId}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id, Feature = Features.Chat.DELETE_CONVERSATION_ID)]
        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(long conversationId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteConversation(conversationId));
        }

        [Route("conversation/get/messages/{id}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetMessagesForConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessagesForConversation(id));
        }

        [Route("message/delete/{messageId}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id, Feature = Features.Chat.DELETE_MESSAGE_ID)]
        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteMessage(messageId));
        }

        [Route("message/get/{id}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpGet]
        public async Task<IActionResult> GetMessage(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessage(id));
        }

        [Route("message/send")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] ChatMessage message)
        {
            return _codeFactory.GetStatusCode(await _chatService.AddMessage(message));
        }

        [Route("message/read/{conversationId}/{messageId}")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPut]
        public async Task<IActionResult> ReadMessage(long conversationId, long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessage(conversationId, messageId));
        }

        [Route("message/read/{conversationId}/list")]
        [ModuleAuthFilterFactory(Module = AddOns.Chat.Id)]
        [HttpPut]
        public async Task<IActionResult> ReadMessageList(long conversationId, [FromBody] long[] messageIds)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessageList(conversationId, messageIds));
        }
    }
}
