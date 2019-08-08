namespace Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.PresentEmployees
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// show present/absent employees today.
    /// </summary>
    [Serializable]
    public class PresentEmployeeCard
    {
        /// <summary>
        /// Show list of employees who are either present or absent, in a card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="punchList">List of punches.</param>
        /// <param name="isHere">Boolean to check whether employee is present.</param>
        /// <param name="message">bot command.</param>
        /// <param name="currentPage">current page value.</param>
        /// <returns>A task.</returns>
        public async Task ShowPresentEmployeesData(IDialogContext context, IEnumerable<KeyValuePair<string, string>> punchList, bool isHere, string message, int currentPage, int totalEmp)
        {
            string title = string.Empty;
            if (isHere)
            {
                // title = $"Showing {(currentPage * 5) - 4} - {((currentPage * 5) - 4) + (punchList.Count() - 1)} of {totalEmp} present employees";
                title = KronosResourceText.ShowPresentEmployeesTxt.Replace("{first}", ((currentPage * 5) - 4).ToString())
                    .Replace("{last}", ((currentPage * 5) - 4 + (punchList.Count() - 1)).ToString())
                    .Replace("{totalEmp}", totalEmp.ToString());
            }
            else
            {
                title = KronosResourceText.ShowAbsentEmployeesTxt.Replace("{first}", ((currentPage * 5) - 4).ToString())
                  .Replace("{last}", ((currentPage * 5) - 4 + (punchList.Count() - 1)).ToString())
                  .Replace("{totalEmp}", totalEmp.ToString());
            }

            if (punchList.Any())
            {
                var dynamicContainer = new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Medium,
                            Weight = AdaptiveTextWeight.Bolder,
                            Text = title,
                        },
                    },
                };

                var dynamicItems = new List<AdaptiveElement>();
                foreach (var punch in punchList)
                {
                    var columnList = new List<AdaptiveColumn>
                    {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                 {
                                     new AdaptiveImage
                                     {
                                         Style = AdaptiveImageStyle.Person,
                                         Url = new Uri("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAASuSURBVGhD7ZpZyFdFGIe/yooWCi1B224iKMMMEyIsBAlaoJWiRXKBjEJaIKKrumhRW4iCsqK6iCIioohWumghKgLLiIQuMrXCNtvLitbnOTUwHM//nJn5L1/K94MHnf85M+c9Z7b3fecbm9B2rANhIdwDL8En8C38Db/BN/A+PAHXw/GwG/wvNAUuhdWgwblsgYfhBNgBRq59YSX8AMGor+Fx8MXmwwGwByiNnAyHw5lwI7wJf0Co/y6cBSORBl0IX4EP/wuegdNgZ8jVfnA1rIfwQg7Lw2Bo2gc0OjzwaZgFg5AfYTFsAtv+BS6CgetQ2AA+5DM4FVK1O+z47387tRfcCfa0z7oPdoKBaA5sBht+BaZBLzn0ToK7YS38CNYTV7EH4Qjo0unwPVjvKdgF+tIM+BJs8FFoa1ADncDB8F64FF8BLhhtOhLsfev47OKecU58DDb0CLQ1ZK99B7HBXfwKy6Gt3UMgfMhb/CFXDpEwsV+Gtp5wLwlfroQHoE1Hg5PfeXOKP+RoKfiQz6FtTih36bpxuRwHbboEvM/ecT9KkmM3TO6U1elDiI0q4SHo0rPgvS4kSboJrOA+0SU3tNigUlzRunQwOMR+B+dOqxzvLpmOx5n+0KFjoMmwXP6EFM/APcb7u+bV2GXgjSm9oY6F2KB+2Bu6dBDYI/ZM6/1vg42mrg6DehGNmwQpCnPFBalRvq03ONFTHUCdu9igUj6CVJ0H1vGFGqXT5g2PVaU0+RXd2GKjSngRUuWq6pwyhGj84PeCjS6rSulaA3XDcrkBcvQeWG92VarpVfDivKqULr9mbFQJF0COdJms5zDbSp+CF/evSmnyXrs5NqqE1FUy6Dqw3jVVqaYQtuYkBI6C2KBS3oEchW3i1qpUkxfcCHM0FWKDSjGzkqMlYL1VVakm13Iv5uoNiI0qwTRSjgyDrXdXVaopxBOGmzkybg8xQwn2Rm7QdCVYd0VVqsnQ1ItGhbnyi8bGpeLH2xVydTtY//KqVJMrhxdzEgtB+j0hq5hD49BIUHBTTq5KNbkpeTF3cwoyPxUb2YVetkm8XBm9hqFsmnYrmbb04utVKV+6KzkT35WnRIYX1l9XlRq0J/wMpjGn+0OBUveVt6BU14JtmCzvKdMu3uSqUKLwtboofRGH1QdgG+aXe+pE8Cbd6tT4IFZqxKjTVyInt/W1rzV76RuHZXiRP2QqZDu6MAmuV5AjbQtz0Ix/p84Fb9aJdN6kyL3gKjAEjQ1uw0MfI8xUnQ/WM2mY5A/65q+Ble7whxaZ3L4NQvqoBA+JLoa2GNxgKmTqfaFkubt7mtSU3fOB+jqD8LFiwumVR3Lx+PfDhqznc/+Vs2SkaGXdCFcjX0DfJj6lGhYOu7NB+Ux/+wK6sp49dT/YiPMlBF6jxDjFf83gz4VimbwOfs144QZ9DvQtX+ZJaHrIsLEnBnpAarxwMzQ9bFg4J/oaTm06AzxqaHrwIHkeTJAPVa7pOmwhNB4kGyE3NdS3TJe6qrn+NxmVg2csRnvj+icdHkX4RwQvwE/QZGgTHnd7aKMXm3p8PTLpMZvGXAAmzwxlTcWKroxf3VA6Jwk4oW1QY2P/ANttdsxdJ2yfAAAAAElFTkSuQmCC"),
                                         Size = AdaptiveImageSize.Small,
                                     },
                                 },
                                Width = "auto",
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                         {
                                             new AdaptiveTextBlock
                                             {
                                                 Weight = AdaptiveTextWeight.Bolder,
                                                 Text = punch.Key,
                                                 Wrap = true,
                                             },
                                             new AdaptiveTextBlock
                                             {
                                                 Weight = AdaptiveTextWeight.Bolder,
                                                 Text = punch.Value,
                                                 Size = AdaptiveTextSize.Small,
                                                 IsSubtle = true,
                                                 Spacing = AdaptiveSpacing.None,
                                                 Wrap = true,
                                             },
                                         },
                                Width = "3",
                            },
                    };

                    dynamicItems.Add(new AdaptiveColumnSet
                    {
                        Columns = columnList,
                        Separator = true,
                    });
                }

                AdaptiveCard card = new AdaptiveCard("1.0");
                dynamicContainer.Items.AddRange(dynamicItems);
                card.Body.Add(dynamicContainer);
                card.Actions.AddRange(this.GetButtons(context, message, currentPage, isHere));

                // show card
                if (message.Contains(Constants.presentEmpPrevpage) || message.Contains(Constants.presentEmpNextpage) || message.Contains(Constants.absentEmpNextpage) || message.Contains(Constants.absentEmpPrevpage))
                {
                    var replyMessage = ((Activity)context.Activity).CreateReply();
                    var conversationId = context.Activity.Conversation.Id;
                    var activityId = context.Activity.ReplyToId;
                    IConnectorClientFactory factory = new ConnectorClientFactory(Address.FromActivity(context.Activity), new MicrosoftAppCredentials(AppSettings.Instance.MicrosoftAppId, AppSettings.Instance.MicrosoftAppPassword));
                    replyMessage.Attachments.Add(new Attachment()
                    {
                        Content = card,
                        ContentType = "application/vnd.microsoft.card.adaptive",
                    });
                    await factory.MakeConnectorClient().Conversations.UpdateActivityAsync(conversationId, activityId, replyMessage);
                }
                else
                {
                    var replyMessage = context.MakeMessage();
                    replyMessage.Attachments = new List<Attachment>
                            {
                                new Attachment()
                                {
                                    Content = card,
                                    ContentType = "application/vnd.microsoft.card.adaptive",
                                },
                            };
                    await context.PostAsync(replyMessage);
                }
            }
            else
            {
                if (isHere)
                {
                    await context.PostAsync(KronosResourceText.NoEmployeesClockedToday);
                }
                else
                {
                    await context.PostAsync(KronosResourceText.NoEmployeesNotClockedToday);
                }
            }
        }

        private List<AdaptiveSubmitAction> GetButtons(IDialogContext context, string message, int currentPage, bool isHere)
        {
            List<AdaptiveSubmitAction> btns = new List<AdaptiveSubmitAction>();
            var pagewiseHashtable = isHere ? context.PrivateConversationData.GetValue<Hashtable>("PagewisePresentAttendance") : context.PrivateConversationData.GetValue<Hashtable>("PagewiseAbsentAttendance");

            if (pagewiseHashtable.Count > 0)
            {
                if (currentPage == 1 && currentPage != pagewiseHashtable.Count)
                {
                    btns.Add(
                        new AdaptiveSubmitAction()
                        {
                            Title = KronosResourceText.Next,
                            Data = new AdaptiveCardAction
                            {
                                msteams = new Msteams
                                {
                                    type = "messageBack",
                                    text = isHere ? Constants.presentEmpNextpage + "/" + currentPage.ToString() : Constants.absentEmpNextpage + "/" + currentPage.ToString(),
                                },
                            },
                        });
                }
                else
                {
                    if (currentPage != 1)
                    {
                        btns.Add(
                           new AdaptiveSubmitAction()
                           {
                               Title = KronosResourceText.Previous.ToLowerInvariant(),
                               Data = new AdaptiveCardAction
                               {
                                   msteams = new Msteams
                                   {
                                       type = "messageBack",
                                       text = isHere ? Constants.presentEmpPrevpage + "/" + currentPage.ToString() : Constants.absentEmpPrevpage + "/" + currentPage.ToString(),
                                   },
                               },
                           });
                    }

                    if (currentPage != pagewiseHashtable.Count)
                    {
                        btns.Add(
                          new AdaptiveSubmitAction()
                          {
                              Title = KronosResourceText.Next,
                              Data = new AdaptiveCardAction
                              {
                                  msteams = new Msteams
                                  {
                                      type = "messageBack",
                                      text = isHere ? Constants.presentEmpNextpage + "/" + currentPage.ToString() : Constants.absentEmpNextpage + "/" + currentPage.ToString(),
                                  },
                              },
                          });
                    }
                }
            }

            return btns;
        }
    }
}