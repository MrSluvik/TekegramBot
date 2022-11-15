using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using Newtonsoft.Json;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using Telegram.Bot.Polling;

namespace Bot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient("5739066192:AAFKuZBUQbgy4YthkPnj9Qyl5KqdBxspKzk");
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions, cancellationToken: cts.Token);
            var me = await botClient.GetMeAsync();

            Console.ReadLine();
            cts.Cancel();
        }

        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var request = new GetRequest("https://api.privatbank.ua/p24api/pubinfo?json&exchange&coursid=5");
            request.Run();//отримав данні в json форматі готівковий курс у банку
            var response = request.Response;
            var cours = JsonConvert.DeserializeObject<List<Bank>>(response);

            var request2 = new GetRequest("https://api.privatbank.ua/p24api/pubinfo?exchange&json&coursid=11");
            request2.Run();//отримав данні в json форматі безготівковий курс у банку
            var response2 = request2.Response;
            var cours2 = JsonConvert.DeserializeObject<List<Bank>>(response2);

            if (update.Message.Text != null)
            {
                await HandleMassage(botClient, update.Message, cours, cours2);

            }
        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        async static Task HandleMassage(ITelegramBotClient botClient, Message message, List<Bank> cours, List<Bank> cours2)//функція для клавіатури
        {
            string name = "[USD] [EUR] [RUR] [BTC]";
            string[] nameCurrency = name.Split(' ');

            string nameCommand = "[USD] [EUR] [RUR] [BTC] USD EUR BTC /start /currency /help /cashless /cash";
            string[] arrNameCommand = nameCommand.Split(' ');
            int n = 0;//зміна для перевірки чи користувач правильно ввів все
            string cur1 = Convert.ToString(message.Text);
            string[] cur2 = cur1.Split(' ');
            int numb = 0;//зміна для кількості грошей які користувач хоче конвертувати 
            double result = 0;//змінна для суми конвертованих грошей
            if (cur2.Length == 4)
            {
                try
                {
                    numb = Convert.ToInt32(cur2[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception {ex}");
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Ви ввели не коректно число або валютну пару");
                    return;
                }
                if (cur2[3].ToUpper() == "BUY")
                {
                    if (cur2[1].ToUpper() == "UAHUSD" && cur2[2].ToUpper() == "CASH")
                    {
                        result = numb / cours[0].buy;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} USD");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "UAHEUR" && cur2[2].ToUpper() == "CASH")
                    {
                        result = numb / cours[1].buy;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} EUR");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "UAHBTC" && cur2[2].ToUpper() == "CASH")
                    {
                        result = numb / cours[0].buy / cours[2].buy;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} BTC");
                        return;
                    }

                    if (cur2[1].ToUpper() == "UAHUSD" && cur2[2].ToUpper() == "CASHLESS")
                    {
                        result = numb / cours2[0].buy;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} USD");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "UAHEUR" && cur2[2].ToUpper() == "CASHLESS")
                    {
                        result = numb / cours2[1].buy;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} EUR");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "UAHBTC" && cur2[2].ToUpper() == "CASHLESS")
                    {
                        result = numb / cours2[0].buy / cours2[3].buy;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} BTC");
                        return;
                    }
                }
                else if (cur2[3].ToUpper() == "SALE")
                {
                    if (cur2[1].ToUpper() == "USDUAH" && cur2[2].ToUpper() == "CASH")
                    {
                        result = numb * cours[0].sale;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} UAH");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "EURUAH" && cur2[2].ToUpper() == "CASH")
                    {
                        result = numb * cours[1].sale;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} UAH");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "BTCUAH" && cur2[2].ToUpper() == "CASH")
                    {
                        result = numb * cours[2].sale * cours[0].sale;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} UAH");
                        return;
                    }

                    if (cur2[1].ToUpper() == "USDUAH" && cur2[2].ToUpper() == "CASHLESS")
                    {
                        result = numb * cours2[0].sale;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} UAH");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "EURUAH" && cur2[2].ToUpper() == "CASHLESS")
                    {
                        result = numb * cours2[1].sale;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} UAH");
                        return;
                    }
                    else if (cur2[1].ToUpper() == "BTCUAH" && cur2[2].ToUpper() == "CASHLESS")
                    {
                        result = numb * cours2[3].sale * cours2[0].sale;
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ви отримаєте {result} UAH");
                        return;
                    }

                }
            }

            foreach (var item in arrNameCommand)
            {
                if (message.Text == item)
                {
                    n++;
                }
            }

            if (n == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Ви ввели :{message.Text}");
                await using Stream stream = System.IO.File.OpenRead(@"D:/Users8/Я/Desktop/1.jpg");
                await botClient.SendPhotoAsync(message.Chat.Id, new InputOnlineFile(stream, "1.jpg"));
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Спробуйте знову ");
                await using Stream stream2 = System.IO.File.OpenRead(@"D:/Users8/Я/Desktop/2.jpg");
                await botClient.SendPhotoAsync(message.Chat.Id, new InputOnlineFile(stream2, "2.jpg"));
                return;
            }


            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ласкаво просимо, {message.Chat.FirstName}! До телеграм боту ExchangesRatesBot.");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Ми надаєм актуальни курс валют який синхронізований з PrivatBank");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Будь ласка,{message.Chat.FirstName}, виберіть одну із команду:/help,/currency - команди допомоги \n/cash , /cashless - комади для роботи.Або введетіть суму грошейяку хочете конвертувати ");

                return;
            }
            if (message.Text == "/currency")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Підтримувані валютні курси:\nГотівковий курс :USD , EUR , BTS\nБезготівковий курс :USD , EUR ,RUR, BTS");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Після вибору команди для роботи виберіть або введіть назву валютидля отримання її курсу,\nдля безготівкового курсу формату:[USD];\nдля готівкового курсу формату: USD");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Підтримувані валютні пари для купівлі/продажу:\nГотівковий курс(CASH) :UAHUSD , UAHEUR ,UAHBTS\nБезготівковий курс(CASHLESS) :USDUAH , EURUAH , BTSUAH");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Якщо хочете купити валюту , введіть за наступним шаблонм: 100 UAHUSD CASH BUY - для купівлі за готівковим курсом \n100 UAHUSD CASHLESS BUY - для купівлі за безготівковим курсом ");
                await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Якщо хочете продати валюту , введіть за наступним шаблонм: 100 UAHUSD CASH SALE  - для продажу по готівковому курсу\n 100 UAHUSD CASHLESS SALE - для продажу по безготівковому курсу ");
                return;
            }
            if (message.Text == "/help")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "/cash команда виклику клавіатури для готівкового курсу валют \n\n /cashless команда виклику клавіатури для безготівкового курсу валют\n\n/currency команда яка надішле перелік підтримувагих валют ");
                return;
            }
            if (message.Text == "/cash")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
               new KeyboardButton[] { "USD", "EUR" },
               new KeyboardButton[] { "BTC"},
            })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть валюту:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            else if (message.Text.ToUpper() == "USD" || message.Text.ToUpper() == "EUR" || message.Text.ToUpper() == "BTC")
            {
                for (int i = 0; i < 3; i++)
                {
                    if (message.Text.ToUpper() == $"{cours[i].ccy}")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Курс продажі {cours[i].sale} {cours[i].base_ccy}\nКурс купівлі{cours[i].buy} {cours[i].base_ccy}");
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            if (message.Text == "/cashless")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
               new KeyboardButton[] { "[USD]", "[EUR]" },
               new KeyboardButton[] { "[RUR]", "[BTC]"},
            })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть валюту:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            else if (message.Text.ToUpper() == "[USD]" || message.Text.ToUpper() == "[EUR]" || message.Text.ToUpper() == "[RUR]" || message.Text.ToUpper() == "[BTC]")
            {
                for (int i = 0; i < 4; i++)
                {
                    if (message.Text.ToUpper() == nameCurrency[i])
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Курс продажі {cours2[i].sale} {cours2[i].base_ccy}\nКурс купівлі{cours2[i].buy}{cours2[i].base_ccy}");
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }
}
