using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp25;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace UtilityBot
{
    class Bot : BackgroundService
    {
        /// <summary>
        /// объект, отвеающий за отправку сообщений клиенту
        /// </summary>
        private readonly Storage _storage;
        private ITelegramBotClient _telegramClient;

        public Bot(ITelegramBotClient telegramClient, Storage storage)
        {
            _telegramClient = telegramClient;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _telegramClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions() { AllowedUpdates = { } }, // receive all update types
                cancellationToken: stoppingToken);

            Console.WriteLine("Bot started");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //  Обрабатываем нажатия на кнопки  из Telegram Bot API: https://core.telegram.org/bots/api#callbackquery
            if (update.Type == UpdateType.CallbackQuery)
            {
                var data = update.CallbackQuery.Data;
                var storage = _storage.GetSession(update.CallbackQuery.From.Id);
                if (data == "len")
                {
                    storage.operation = Operation.Длина;
                }
                else if (data == "sum")
                {
                    storage.operation = Operation.Сумма;
                }
                return;
            }

            // Обрабатываем входящие сообщения из Telegram Bot API: https://core.telegram.org/bots/api#message
            if (update.Type == UpdateType.Message)
            {
                switch (update.Message!.Type)
                {
                    case MessageType.Text:
                        if (update.Message.Text == "/start")
                        {
                            var buttons = new List<InlineKeyboardButton[]>
                                {
                                    new[]
                                {
                                    InlineKeyboardButton.WithCallbackData($"Длина слова" , $"len"),
                                    InlineKeyboardButton.WithCallbackData($"Сумма" , $"sum")
                                }};
                            await _telegramClient.SendTextMessageAsync(update.Message.Chat.Id, $"Выбери кнопку", cancellationToken: cancellationToken, replyMarkup: new InlineKeyboardMarkup(buttons));
                        }
                        else
                        {
                            var storage = _storage.GetSession(update.Message.Chat.Id);
                            switch (storage.operation)
                            {
                                case Operation.Длина:
                                    await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Длина сообщения: {update.Message.Text.Length} знаков", cancellationToken: cancellationToken);
                                    break;
                                case Operation.Сумма:
                                    await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Сумма сообщения: {update.Message.Text.Split(" ").AsEnumerable().Select((str) => { int num; int.TryParse(str, out num); return num; }).Sum()}", cancellationToken: cancellationToken);
                                    break;
                            }
                        }
                        return;

                    default: // unsupported message
                        await _telegramClient.SendTextMessageAsync(update.Message.From.Id, $"Данный тип сообщений не поддерживается. Пожалуйста отправьте текст.", cancellationToken: cancellationToken);
                        return;
                }
            }

        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            Console.WriteLine("Ожидаем 10 секунд перед повторным подключением.");
            Thread.Sleep(10000);
            return Task.CompletedTask;
        }
    }
}