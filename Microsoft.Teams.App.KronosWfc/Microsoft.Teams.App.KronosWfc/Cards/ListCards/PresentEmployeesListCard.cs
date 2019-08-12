namespace Microsoft.Teams.App.KronosWfc.Cards.ListCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Resources;

    /// <summary>
    /// show present/absent employees.
    /// </summary>
    [Serializable]
    public class PresentEmployeesListCard
    {
        /// <summary>
        /// Show list of employees who are either present or absent, in a card.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="punchList">List of punches.</param>
        /// <param name="isHere">Boolean to check whether employee is present.</param>
        /// <returns>A task.</returns>
        public async Task ShowPresentEmployeesData(IDialogContext context, IEnumerable<KeyValuePair<string, string>> punchList, bool isHere, string message, int currentPage)
        {
            string title = isHere ? KronosResourceText.HereAreEmpWhoAreHere : KronosResourceText.HereAreEmpWhoAreNotHere;
            if (punchList.Any())
            {
                var list = new List<Item>();

                foreach (var punch in punchList)
                {
                    list.Add(new Item()
                    {
                        title = punch.Key,
                        icon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAASuSURBVGhD7ZpZyFdFGIe/yooWCi1B224iKMMMEyIsBAlaoJWiRXKBjEJaIKKrumhRW4iCsqK6iCIioohWumghKgLLiIQuMrXCNtvLitbnOTUwHM//nJn5L1/K94MHnf85M+c9Z7b3fecbm9B2rANhIdwDL8En8C38Db/BN/A+PAHXw/GwG/wvNAUuhdWgwblsgYfhBNgBRq59YSX8AMGor+Fx8MXmwwGwByiNnAyHw5lwI7wJf0Co/y6cBSORBl0IX4EP/wuegdNgZ8jVfnA1rIfwQg7Lw2Bo2gc0OjzwaZgFg5AfYTFsAtv+BS6CgetQ2AA+5DM4FVK1O+z47387tRfcCfa0z7oPdoKBaA5sBht+BaZBLzn0ToK7YS38CNYTV7EH4Qjo0unwPVjvKdgF+tIM+BJs8FFoa1ADncDB8F64FF8BLhhtOhLsfev47OKecU58DDb0CLQ1ZK99B7HBXfwKy6Gt3UMgfMhb/CFXDpEwsV+Gtp5wLwlfroQHoE1Hg5PfeXOKP+RoKfiQz6FtTih36bpxuRwHbboEvM/ecT9KkmM3TO6U1elDiI0q4SHo0rPgvS4kSboJrOA+0SU3tNigUlzRunQwOMR+B+dOqxzvLpmOx5n+0KFjoMmwXP6EFM/APcb7u+bV2GXgjSm9oY6F2KB+2Bu6dBDYI/ZM6/1vg42mrg6DehGNmwQpCnPFBalRvq03ONFTHUCdu9igUj6CVJ0H1vGFGqXT5g2PVaU0+RXd2GKjSngRUuWq6pwyhGj84PeCjS6rSulaA3XDcrkBcvQeWG92VarpVfDivKqULr9mbFQJF0COdJms5zDbSp+CF/evSmnyXrs5NqqE1FUy6Dqw3jVVqaYQtuYkBI6C2KBS3oEchW3i1qpUkxfcCHM0FWKDSjGzkqMlYL1VVakm13Iv5uoNiI0qwTRSjgyDrXdXVaopxBOGmzkybg8xQwn2Rm7QdCVYd0VVqsnQ1ItGhbnyi8bGpeLH2xVydTtY//KqVJMrhxdzEgtB+j0hq5hD49BIUHBTTq5KNbkpeTF3cwoyPxUb2YVetkm8XBm9hqFsmnYrmbb04utVKV+6KzkT35WnRIYX1l9XlRq0J/wMpjGn+0OBUveVt6BU14JtmCzvKdMu3uSqUKLwtboofRGH1QdgG+aXe+pE8Cbd6tT4IFZqxKjTVyInt/W1rzV76RuHZXiRP2QqZDu6MAmuV5AjbQtz0Ix/p84Fb9aJdN6kyL3gKjAEjQ1uw0MfI8xUnQ/WM2mY5A/65q+Ble7whxaZ3L4NQvqoBA+JLoa2GNxgKmTqfaFkubt7mtSU3fOB+jqD8LFiwumVR3Lx+PfDhqznc/+Vs2SkaGXdCFcjX0DfJj6lGhYOu7NB+Ux/+wK6sp49dT/YiPMlBF6jxDjFf83gz4VimbwOfs144QZ9DvQtX+ZJaHrIsLEnBnpAarxwMzQ9bFg4J/oaTm06AzxqaHrwIHkeTJAPVa7pOmwhNB4kGyE3NdS3TJe6qrn+NxmVg2csRnvj+icdHkX4RwQvwE/QZGgTHnd7aKMXm3p8PTLpMZvGXAAmzwxlTcWKroxf3VA6Jwk4oW1QY2P/ANttdsxdJ2yfAAAAAElFTkSuQmCC",
                        subtitle = punch.Value,
                        type = Constants.ItemType,
                    });
                }

                await this.ShowListCard(context, list, title, message, currentPage, isHere);
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

        /// <summary>
        /// Show generic list card based on commands.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="list">Items to show on the list card.</param>
        /// <param name="message">message to display on the header of the list card.</param>
        /// <returns>A Task.</returns>
        public async Task ShowListCard(IDialogContext context, List<Item> list, string title, string message, int currentPage, bool isHere)
        {
            var repoMessage = context.MakeMessage();
            if (list.Count == 0)
            {
                await context.PostAsync(KronosResourceText.None);
                return;
            }

            List<Button> btns = this.GetButtons(context, message, currentPage, isHere);
            Attachment attachment = this.GetAttachment(list, title, message, btns);
            repoMessage.Attachments.Add(attachment);
            await context.PostAsync(repoMessage);
        }

        /// <summary>
        /// Prepare the attachments to be displayed in the list card.
        /// </summary>
        /// <param name="list">Items to show on the list card.</param>
        /// <param name="message">message to display on the header of the list card.</param>
        /// <returns>An Attachment</returns>
        public Attachment GetAttachment(List<Item> list, string title, string message, List<Button> btns)
        {
            var listCard = new ListCard()
            {
                content = new Content()
                {
                    title = title,
                    items = list.ToArray(),
                    buttons = btns.ToArray(),
                },
            };

            Attachment attachment = new Attachment();
            attachment.ContentType = listCard.contentType;
            attachment.Content = listCard.content;
            return attachment;
        }

        private List<Button> GetButtons(IDialogContext context, string message, int currentPage, bool isHere)
        {
            List<Button> btns = new List<Button>();
            var pagewiseHashtable = context.PrivateConversationData.GetValue<Hashtable>("PagewiseAttendance");

            if (pagewiseHashtable.Count > 0)
            {
                if (currentPage == 1 && currentPage != pagewiseHashtable.Count)
                {
                    btns.Add(new Button()
                    {
                        type = "messageBack",
                        text = isHere ? Constants.presentEmpNextpage + "/" + currentPage.ToString() : Constants.absentEmpNextpage + "/" + currentPage.ToString(),
                        title = "Show next 15",
                    });
                }
                else
                {
                    if (currentPage != 1)
                    {
                        btns.Add(new Button()
                        {
                            type = "messageBack",
                            text = isHere ? Constants.presentEmpPrevpage + "/" + currentPage.ToString() : Constants.absentEmpPrevpage + "/" + currentPage.ToString(),
                            title = "Show prev 15",
                        });
                    }

                    if (currentPage != pagewiseHashtable.Count)
                    {
                        btns.Add(new Button()
                        {
                            type = "messageBack",
                            text = isHere ? Constants.presentEmpNextpage + "/" + currentPage.ToString() : Constants.absentEmpNextpage + "/" + currentPage.ToString(),
                            title = "Show next 15",
                        });
                    }
                }
            }

            return btns;
        }
    }
}