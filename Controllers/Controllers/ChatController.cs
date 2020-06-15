using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Factory.Interface;
using VueServer.Models.Chat;
using VueServer.Models.Request;
using VueServer.Services.Interface;

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
        [HttpPost]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.StartConversation(request));
        }

        [Route("conversation/get/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetConversation(Guid id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetConversation(id));
        }

        [Route("conversation/get-all/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetAllConversations(string userId)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetAllConversations(userId));
        }


        [Route("message/get/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetMessage(Guid id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessage(id));
        }

        [Route("message/send")]
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] ChatMessage message)
        {
            return _codeFactory.GetStatusCode(await _chatService.AddMessage(message));
        }
    }
}
