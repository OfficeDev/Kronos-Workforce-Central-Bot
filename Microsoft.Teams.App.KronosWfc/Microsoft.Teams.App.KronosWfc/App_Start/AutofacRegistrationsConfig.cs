//-----------------------------------------------------------------------
// <copyright file="AutofacRegistrationsConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Teams.App.KronosWfc.App_Start
{
    using System.Reflection;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.AzureEntity;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.CommentList;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Common;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Core;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Hours;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.HyperFind;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.JobAssignment;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Punch;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Role;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Shifts;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.SupervisorViewTimeOff;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.SwapShift;
    using Microsoft.Teams.App.KronosWfc.BusinessLogic.Vacation;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Punches;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Schedule;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.SupervisorViewTimeOffRequests;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.SwapShift;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.TeamOvertimesCard;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.VacationBalance;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.ViewTimeOffRequests;
    using Microsoft.Teams.App.KronosWfc.Cards.AdaptiveCards.Welcome;
    using Microsoft.Teams.App.KronosWfc.Cards.CarouselCards;
    using Microsoft.Teams.App.KronosWfc.Cards.HeroCards;
    using Microsoft.Teams.App.KronosWfc.Cards.ListCards;
    using Microsoft.Teams.App.KronosWfc.CommandHandling;
    using Microsoft.Teams.App.KronosWfc.Common;
    using Microsoft.Teams.App.KronosWfc.Controllers;
    using Microsoft.Teams.App.KronosWfc.Controllers.Login;
    using Microsoft.Teams.App.KronosWfc.Dialogs;
    using Microsoft.Teams.App.KronosWfc.Dialogs.EmployeeLocation;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Hours;
    using Microsoft.Teams.App.KronosWfc.Dialogs.OnLeave;
    using Microsoft.Teams.App.KronosWfc.Dialogs.PresentEmployees;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Punch;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Schedule;
    using Microsoft.Teams.App.KronosWfc.Dialogs.SupervisorViewTimeoff;
    using Microsoft.Teams.App.KronosWfc.Dialogs.SwapShift;
    using Microsoft.Teams.App.KronosWfc.Dialogs.TeamOvertimes;
    using Microsoft.Teams.App.KronosWfc.Dialogs.TimeOff;
    using Microsoft.Teams.App.KronosWfc.Dialogs.Vacation;
    using Microsoft.Teams.App.KronosWfc.Models;
    using Microsoft.Teams.App.KronosWfc.Models.RequestEntities.Logon;
    using Microsoft.Teams.App.KronosWfc.Provider;
    using Microsoft.Teams.App.KronosWfc.Provider.Core;

    /// <summary>
    /// Register all the dependencies in the container.
    /// </summary>
    public class AutofacRegistrationsConfig
    {
        /// <summary>
        /// Method to register all the dependencies in the container.
        /// </summary>
        public static void ConfigureAutofacRegistrations()
        {
            var store = new TableBotDataStore(new AzureTableStorageHelper().StorageAccount);
            Conversation.UpdateContainer(
                       builder =>
                       {
                           builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                           builder.Register(c => store)
                                     .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                                     .AsSelf()
                                     .SingleInstance();

                           builder.Register(
                               c => new CachingBotDataStore(
                                      store,
                                      CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                                      .As<IBotDataStore<BotData>>()
                                      .AsSelf()
                                      .InstancePerLifetimeScope();

                           builder.RegisterType<RootDialog>()
                               .As<IDialog<object>>()
                               .InstancePerDependency();

                           builder.RegisterType<DialogFactory>()
                            .Keyed<IDialogFactory>(FiberModule.Key_DoNotSerialize)
                            .AsImplementedInterfaces()
                            .InstancePerLifetimeScope();

                           builder.RegisterType<DialogContext>()
                           .InstancePerDependency();

                           builder.RegisterType<AddPunchDialog>().InstancePerDependency();
                           builder.RegisterType<VacationBalanceDialog>().InstancePerDependency();
                           builder.RegisterType<ScheduleDialog>().InstancePerDependency();
                           builder.RegisterType<SavePunchDialog>().InstancePerDependency();
                           builder.RegisterType<SaveWorkRuleTransferDialog>().InstancePerDependency();
                           builder.RegisterType<UpcomingShiftsDialog>().InstancePerDependency();
                           builder.RegisterType<ShowPunchesDialog>().InstancePerDependency();
                           builder.RegisterType<ShowHoursWorkedDialog>().InstancePerDependency();
                           builder.RegisterType<PresentEmployeesDialog>().InstancePerDependency();
                           builder.RegisterType<EmployeeLocationDialog>().InstancePerDependency();
                           builder.RegisterType<OnLeaveDialog>().InstancePerDependency();
                           builder.RegisterType<TeamOvertimesDialog>().InstancePerDependency();

                           builder.RegisterType<AddPunchActivity>().As<IAddPunchActivity>().InstancePerDependency();
                           builder.RegisterType<ViewVacationBalanceActivity>().As<IViewVacationBalanceActivity>().InstancePerDependency();
                           builder.RegisterType<UpcomingShiftsActivity>().As<IUpcomingShiftsActivity>().InstancePerDependency();

                           builder.RegisterType<ShowPunchesActivity>().As<IShowPunchesActivity>().InstancePerDependency();
                           builder.RegisterType<HoursWorkedActivity>().As<IHoursWorkedActivity>().InstancePerDependency();
                           builder.RegisterType<PresentEmployeesActivity>().As<IPresentEmployeesActivity>().InstancePerDependency();
                           builder.RegisterType<RoleActivity>().As<IRoleActivity>().InstancePerDependency();
                           builder.RegisterType<HyperFindActivity>().As<IHyperFindActivity>().InstancePerDependency();
                           builder.RegisterType<JobAssignmentActivity>().As<IJobAssignmentActivity>().InstancePerDependency();

                           builder.RegisterType<TimeOffDialog>().InstancePerDependency();
                           builder.RegisterType<TimeOffActivity>().As<ITimeOffActivity>().InstancePerDependency();
                           builder.RegisterType<TimeOffRequestCard>().AsSelf().InstancePerDependency();

                           builder.RegisterType<SupervisorViewTimeOffDialog>().InstancePerDependency();
                           builder.RegisterType<SupervisorViewTimeOffActivity>().As<ISupervisorViewTimeOffActivity>().InstancePerDependency();
                           builder.RegisterType<SupervisorViewTimeOffRequestsCard>().AsSelf().InstancePerDependency();

                           builder.RegisterType<ViewTimeOffRequestsDialog>().InstancePerDependency();
                           builder.RegisterType<ViewTimeOffRequestsCard>().AsSelf().InstancePerDependency();
                           builder.RegisterType<NextVacationCard>().AsSelf().InstancePerDependency();

                           builder.RegisterType<ScheduleActivity>().As<IScheduleActivity>().InstancePerDependency();
                           builder.RegisterType<CommentsActivity>().As<ICommentsActivity>().InstancePerDependency();

                           builder.RegisterType<AzureTableStorageHelper>().As<IAzureTableStorageHelper>().InstancePerDependency();
                           builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();
                           builder.RegisterType<LogonActivity>().As<ILogonActivity>();

                           builder.RegisterType<Request>().AsSelf().InstancePerDependency();
                           builder.RegisterType<LoginResponse>().AsSelf().InstancePerDependency();
                           builder.RegisterType<CarouselHelp>().AsSelf().InstancePerDependency();
                           builder.RegisterType<HeroLeaveCard>().AsSelf().InstancePerDependency();

                           builder.RegisterType<CarouselVacationBalance>().AsSelf().InstancePerDependency();
                           builder.RegisterType<HeroAddPunch>().AsSelf().InstancePerDependency();
                           builder.RegisterType<AuthenticateUser>().AsSelf().InstancePerDependency();
                           builder.RegisterType<HeroShowSchedule>().AsSelf().InstancePerDependency();
                           builder.RegisterType<HeroShowPunches>().AsSelf().InstancePerDependency();
                           builder.RegisterType<CarouselShowPunches>().AsSelf().InstancePerDependency();
                           builder.RegisterType<HeroHoursWorked>().AsSelf().InstancePerDependency();
                           builder.RegisterType<CarouselShowHoursWorked>().AsSelf().InstancePerDependency();
                           builder.RegisterType<AdaptiveDateRange>().AsSelf().InstancePerDependency();
                           builder.RegisterType<CarouselUpcomingShifts>().AsSelf().InstancePerDependency();
                           builder.RegisterType<PresentEmployeeCard>().AsSelf().InstancePerDependency();
                           builder.RegisterType<HeroEmployeeLocation>().AsSelf().InstancePerDependency();
                           builder.RegisterType<CarouselTeamOvertimes>().AsSelf().InstancePerDependency();
                           builder.RegisterType<TeamOvertimesCard>().AsSelf().InstancePerDependency();
                           builder.RegisterType<AdaptiveVacationBalance>().AsSelf().InstancePerDependency();
                           builder.RegisterType<AdaptiveSchedule>().AsSelf().InstancePerDependency();
                           builder.RegisterType<AdaptiveShowPunches>().AsSelf().InstancePerDependency();
                           builder.RegisterType<SwapShiftDialog>().InstancePerDependency();
                           builder.RegisterType<SwapShiftCard>().AsSelf().InstancePerDependency();
                           builder.RegisterType<BotUserEntity>()
                            .Keyed<BotUserEntity>(FiberModule.Key_DoNotSerialize)
                            .AsSelf()
                            .InstancePerDependency();

                           builder.RegisterType<SwapShiftActivity>().As<ISwapShiftActivity>().InstancePerDependency();
                           builder.RegisterType<CommonActivity>().As<ICommonActivity>().InstancePerDependency();

                           builder.RegisterControllers(typeof(LoginController).Assembly);

                           builder.Register(c => new MicrosoftAppCredentials(
                                        CredentialProvider.MicrosoftAppId,
                                        CredentialProvider.MicrosoftAppPassword))
                                    .SingleInstance();

                           builder.RegisterType<WelcomeCard>().AsSelf().InstancePerDependency();
                       });

            // Resolve mvc controller
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Conversation.Container));
        }
    }
}