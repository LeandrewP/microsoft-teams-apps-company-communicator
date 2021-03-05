﻿// <copyright file="SentNotificationsControllerTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Test.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ExportData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.PrepareToSendQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.CompanyCommunicator.Controllers;
    using Microsoft.Teams.Apps.CompanyCommunicator.Controllers.Options;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;
    using Moq;
    using Xunit;

    /// <summary>
    /// SentNotificationsController test class.
    /// </summary>
    public class SentNotificationsControllerTest
    {
        private readonly Mock<INotificationDataRepository> notificationDataRepository = new Mock<INotificationDataRepository>();
        private readonly Mock<ISentNotificationDataRepository> sentNotificationDataRepository = new Mock<ISentNotificationDataRepository>();
        private readonly Mock<ITeamDataRepository> teamDataRepository = new Mock<ITeamDataRepository>();
        private readonly Mock<IPrepareToSendQueue> prepareToSendQueue = new Mock<IPrepareToSendQueue>();
        private readonly Mock<IDataQueue> dataQueue = new Mock<IDataQueue>();
        private readonly Mock<IOptions<DataQueueMessageOptions>> dataQueueMessageOptions = new Mock<IOptions<DataQueueMessageOptions>>();
        private readonly Mock<IGroupsService> groupsService = new Mock<IGroupsService>();
        private readonly Mock<IExportDataRepository> exportDataRepository = new Mock<IExportDataRepository>();
        private readonly Mock<IAppCatalogService> appCatalogService = new Mock<IAppCatalogService>();
        private readonly Mock<IAppSettingsService> appSettingsService = new Mock<IAppSettingsService>();
        // private readonly Mock<UserAppOptions> userAppOptions = new Mock<UserAppOptions>();
        private readonly Mock<IOptions<UserAppOptions>> userAppOptions = new Mock<IOptions<UserAppOptions>>();
        //private readonly Mock<ILogger<SentNotificationsController>> logger = new Mock<ILogger<SentNotificationsController>>();
        private readonly Mock<ILoggerFactory> loggerFactory = new Mock<ILoggerFactory>();

        //private readonly Mock<IOptions<AuthenticationOptions>> options = new Mock<IOptions<AuthenticationOptions>>();

        /// <summary>
        /// Constructor test for all parameters.
        /// </summary>
        [Fact]
        public void CreateInstance_AllParameters_ShouldBeSuccess()
        {
            // Arrange
            Action action = () => GetControllerInstance();

            // Act and Assert.
            action.Should().NotThrow();
        }

        // <summary>
        /// Constructor test for null parameter.
        /// </summary> 
        [Fact]
        public void CreateInstance_NullParameter_ThrowsArgumentNullException()
        {
            // Arrange
            dataQueueMessageOptions.Setup(x => x.Value).Returns(new DataQueueMessageOptions() { ForceCompleteMessageDelayInSeconds = 100 });
            userAppOptions.Setup(x => x.Value).Returns(new UserAppOptions() { ProactivelyInstallUserApp = false });
            Action action1 = () => new SentNotificationsController(null /*notificationDataRepository*/, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action2 = () => new SentNotificationsController(notificationDataRepository.Object, null /*sentNotificationDataRepository*/, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action3 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, null/*teamDataRepository*/, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action4 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, null/*prepareToSendQueue*/, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action5 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, null/*dataQueue*/, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action6 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, null/*dataQueueMessageOptions*/, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action7 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, null/*groupsService*/, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action8 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, null/*exportDataRepository*/, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action9 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, null/*appCatalogServicet*/, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            Action action10 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, null/*appSettingsService*/, userAppOptions.Object, loggerFactory.Object);
            Action action11 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, null/*userAppOptions*/, loggerFactory.Object);
            Action action12 = () => new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, null/*loggerFactory*/);

            // Act and Assert.
            action1.Should().Throw<ArgumentNullException>("notificationDataRepository is null.");
            action2.Should().Throw<ArgumentNullException>("sentNotificationDataRepository is null.");
            action3.Should().Throw<ArgumentNullException>("teamDataRepository is null.");
            action4.Should().Throw<ArgumentNullException>("prepareToSendQueue is null.");
            action5.Should().Throw<ArgumentNullException>("dataQueue is null.");
            action6.Should().Throw<ArgumentNullException>("dataQueueMessageOptions is null.");
            action7.Should().Throw<ArgumentNullException>("groupsService is null.");
            action8.Should().Throw<ArgumentNullException>("exportDataRepository is null.");
            action9.Should().Throw<ArgumentNullException>("appCatalogService is null.");
            action10.Should().Throw<ArgumentNullException>("appSettingsService is null.");
            action11.Should().Throw<ArgumentNullException>("userAppOptions is null.");
            action12.Should().Throw<ArgumentNullException>("authenticationOptions is null.");
        }

        /// <summary>
        /// Test case for null parameter input throws ArgumentNullException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendNotification_NullParameter_ThrowsArgumentNullExceptioin()
        {
            // Arrange
            var controller = GetControllerInstance();

            //Act
            Func<Task> task = async () => await controller.CreateSentNotificationAsync(null /*draftNotification*/);

            //Assert
            await task.Should().ThrowAsync<ArgumentNullException>("draftNotification is null");
        }

        /// <summary>
        /// Test case for null parameter input throws ArgumentNullException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendNotification_NullParameter_ThrowsArgumentNullExceptioin12()
        {
            // Arrange
            var controller = GetControllerInstance();
            var draftNotification = new DraftNotification() { Id = "id" };
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));

            // Act
            var result = await controller.CreateSentNotificationAsync(draftNotification);
            var errorMessage = ((ObjectResult)result).Value;

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(errorMessage, $"Draft notification, Id: {draftNotification.Id}, could not be found.");
        }

        /// <summary>
        /// Test case to verify to check SetUserAppId not invoked when proactive installation is not enable.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAppId_ProactiveInstallationDisabled_SetUserAppIdShouldNeverInvoked()
        {
            // Arrange
            var controller = GetControllerInstance(false);
            var draftNotification = new DraftNotification() { Id = "id" };
            var sentNotificationId = "notificationId";
            var notificationDataEntity = new NotificationDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.MoveDraftToSentPartitionAsync(It.IsAny<NotificationDataEntity>())).ReturnsAsync(sentNotificationId);
            sentNotificationDataRepository.Setup(x => x.EnsureSentNotificationDataTableExistsAsync()).Returns(Task.CompletedTask);
            appSettingsService.Setup(x => x.SetUserAppIdAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            await controller.CreateSentNotificationAsync(draftNotification);

            // Assert
            appSettingsService.Verify(x => x.SetUserAppIdAsync(It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Test case to verify to check SetUserAppId not invoked when appId is already synced.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAppId_AppIdAlreadySynced_SetUserAppIdShouldNeverInvoked()
        {
            // Arrange
            var controller = GetControllerInstance();
            var draftNotification = new DraftNotification() { Id = "id" };
            var sentNotificationId = "notificationId";
            var appId = "appId";
            var notificationDataEntity = new NotificationDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.MoveDraftToSentPartitionAsync(It.IsAny<NotificationDataEntity>())).ReturnsAsync(sentNotificationId);
            sentNotificationDataRepository.Setup(x => x.EnsureSentNotificationDataTableExistsAsync()).Returns(Task.CompletedTask);
            appSettingsService.Setup(x => x.GetUserAppIdAsync()).ReturnsAsync(appId);
            appSettingsService.Setup(x => x.SetUserAppIdAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            await controller.CreateSentNotificationAsync(draftNotification);

            // Assert
            appSettingsService.Verify(x => x.SetUserAppIdAsync(It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Test case to verify SetUserAppIdAsync is not called when invalid externalId is passed to GetTeamsAppIdAsync.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAppId_InvalidExternalId_SetUserAppIdShouldNeverInvoked()
        {
            // Arrange
            var controller = GetControllerInstance();
            var draftNotification = new DraftNotification() { Id = "id" };
            var sentNotificationId = "notificationId";
            string appId = null;
            var notificationDataEntity = new NotificationDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.MoveDraftToSentPartitionAsync(It.IsAny<NotificationDataEntity>())).ReturnsAsync(sentNotificationId);
            sentNotificationDataRepository.Setup(x => x.EnsureSentNotificationDataTableExistsAsync()).Returns(Task.CompletedTask);
            appSettingsService.Setup(x => x.GetUserAppIdAsync()).ReturnsAsync(appId);
            appSettingsService.Setup(x => x.SetUserAppIdAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            appCatalogService.Setup(x => x.GetTeamsAppIdAsync(It.IsAny<string>())).ReturnsAsync(appId);

            // Act
            await controller.CreateSentNotificationAsync(draftNotification);

            // Assert
            appSettingsService.Verify(x => x.SetUserAppIdAsync(It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Test case to verify SetUserAppIdAsync is not called when invalid externalId is passed to GetTeamsAppIdAsync.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_SetUserAppIdServiceCall_ShouldInvokedOnce()
        {
            // Arrange
            var controller = GetControllerInstance();
            var draftNotification = new DraftNotification() { Id = "id" };
            var sentNotificationId = "notificationId";
            string appId = null;
            var teamsAppId = "appId";
            var notificationDataEntity = new NotificationDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.MoveDraftToSentPartitionAsync(It.IsAny<NotificationDataEntity>())).ReturnsAsync(sentNotificationId);
            sentNotificationDataRepository.Setup(x => x.EnsureSentNotificationDataTableExistsAsync()).Returns(Task.CompletedTask);
            appSettingsService.Setup(x => x.GetUserAppIdAsync()).ReturnsAsync(appId);
            appSettingsService.Setup(x => x.SetUserAppIdAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            appCatalogService.Setup(x => x.GetTeamsAppIdAsync(It.IsAny<string>())).ReturnsAsync(teamsAppId);

            // Act
            await controller.CreateSentNotificationAsync(draftNotification);

            // Assert
            appSettingsService.Verify(x => x.SetUserAppIdAsync(It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// Test case to verify SetUserAppIdAsync is not called when invalid externalId is passed to GetTeamsAppIdAsync.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAppId_GraphServiceError_GotServiceExceptionAndSetUserAppIdShouldInvokedOnce()
        {
            // Arrange
            var controller = GetControllerInstance();
            var draftNotification = new DraftNotification() { Id = "id" };
            var sentNotificationId = "notificationId";
            string appId = null;
            var teamsAppId = "appId";
            var notificationDataEntity = new NotificationDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.MoveDraftToSentPartitionAsync(It.IsAny<NotificationDataEntity>())).ReturnsAsync(sentNotificationId);
            sentNotificationDataRepository.Setup(x => x.EnsureSentNotificationDataTableExistsAsync()).Returns(Task.CompletedTask);
            appSettingsService.Setup(x => x.GetUserAppIdAsync()).ReturnsAsync(appId);
            appCatalogService.Setup(x => x.GetTeamsAppIdAsync(It.IsAny<string>())).ReturnsAsync(teamsAppId);

            var serviceException = new ServiceException(null, null, HttpStatusCode.Unauthorized);
            appCatalogService.Setup(x => x.GetTeamsAppIdAsync(It.IsAny<string>())).ThrowsAsync(serviceException);

            // Act
            await controller.CreateSentNotificationAsync(draftNotification);

            // Assert
            appSettingsService.Verify(x => x.SetUserAppIdAsync(It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Test case to verify status code Ok 200 for the send notification for valid data.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendNotification_ForValidData_ReturnsStatusCodeOk()
        {
            // Arrange
            var controller = GetControllerInstance();
            var draftNotification = new DraftNotification() { Id = "id" };
            var sentNotificationId = "notificationId";
            string appId = null;
            var teamsAppId = "appId";
            var statusCodeOk = 200;
            var notificationDataEntity = new NotificationDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.MoveDraftToSentPartitionAsync(It.IsAny<NotificationDataEntity>())).ReturnsAsync(sentNotificationId);
            sentNotificationDataRepository.Setup(x => x.EnsureSentNotificationDataTableExistsAsync()).Returns(Task.CompletedTask);
            appSettingsService.Setup(x => x.GetUserAppIdAsync()).ReturnsAsync(appId);
            appSettingsService.Setup(x => x.SetUserAppIdAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            appCatalogService.Setup(x => x.GetTeamsAppIdAsync(It.IsAny<string>())).ReturnsAsync(teamsAppId);
            prepareToSendQueue.Setup(x => x.SendAsync(It.IsAny<PrepareToSendQueueMessageContent>())).Returns(Task.CompletedTask);
            dataQueue.Setup(x => x.SendDelayedAsync(It.IsAny<DataQueueMessageContent>(), It.IsAny<double>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateSentNotificationAsync(draftNotification);
            var statusCode = ((StatusCodeResult)result).StatusCode;

            // Assert
            Assert.Equal(statusCode, statusCodeOk);
        }

        /// <summary>
        /// Test case to verify return object mapping is correct.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSummary_CorrectMapping_ReturnsNotificationSummaryListObject()
        {
            // Arrange
            var controller = GetControllerInstance();
            var notifications = new List<NotificationDataEntity>() { getNotification() };
            var notification = notifications.FirstOrDefault();
            notificationDataRepository.Setup(x => x.GetMostRecentSentNotificationsAsync()).ReturnsAsync(notifications);

            // Act
            var result = await controller.GetSentNotificationsAsync();
            var sentNotificationSummaryList = result.ToList();
            var sentNotificationSummary = sentNotificationSummaryList.FirstOrDefault();

            // Assert
            Assert.Equal(notification.Id, sentNotificationSummary.Id);
            Assert.Equal(notification.Title, sentNotificationSummary.Title);
            Assert.Equal(notification.SentDate, sentNotificationSummary.SentDate);
            Assert.Equal(notification.Failed, sentNotificationSummary.Failed);
            Assert.Equal(notification.TotalMessageCount, sentNotificationSummary.TotalMessageCount);
            Assert.Equal(notification.SendingStartedDate, sentNotificationSummary.SendingStartedDate);
            Assert.Equal(notification.Status, sentNotificationSummary.Status);
            Assert.Equal(notification.Unknown, sentNotificationSummary.Unknown);
        }

        /// <summary>
        /// Test case to pass null parameter throws ArgumentNullException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNotication_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetControllerInstance();

            // Act
            Func<Task> task = async () => await controller.GetSentNotificationByIdAsync(null /*id*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("id is null");
        }

        /// <summary>
        /// Test case to pass invalid id returns status code not found.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNotication_ForInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var controller = GetControllerInstance();
            string id = "invalidId";
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));

            // Act
            var result = await controller.GetSentNotificationByIdAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Test case to pass valid parameter gives sataus code 200.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNotication_ValidIdParam_ReturnsOkResult()
        {
            // Arrange
            var controller = GetControllerInstance();
            string id = "invalidId";
            var notificationDataEntity = getNotification();
            var groupList = new List<Group>() { new Group() };
            var exportDataEntity = new ExportDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            groupsService.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(groupList.ToAsyncEnumerable());
            exportDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(exportDataEntity);
            teamDataRepository.Setup(x => x.GetTeamNamesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(notificationDataEntity.Teams);

            // Act
            var result = await controller.GetSentNotificationByIdAsync(id);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// Test case to pass valid parameter gives sataus code 200.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNotication_CorrectMapping_ReturnsSentNotificationObject()
        {
            // Arrange
            var controller = GetControllerInstance();
            string id = "invalidId";
            var notificationEntity = getNotification();
            var groupList = new List<Group>() { new Group() { DisplayName = "group1" }, new Group() { DisplayName = "group2" } };
            var exportDataEntity = new ExportDataEntity();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationEntity);
            groupsService.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(groupList.ToAsyncEnumerable());
            exportDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(exportDataEntity);
            teamDataRepository.Setup(x => x.GetTeamNamesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(notificationEntity.Teams);

            // Act
            var result = await controller.GetSentNotificationByIdAsync(id);
            var sentNotification = (SentNotification)((ObjectResult)result).Value;

            // Assert
            Assert.Equal(notificationEntity.Id, sentNotification.Id);
            Assert.Equal(notificationEntity.Title, sentNotification.Title);
            Assert.Equal(notificationEntity.ImageLink, sentNotification.ImageLink);
            Assert.Equal(notificationEntity.Summary, sentNotification.Summary);
            Assert.Equal(notificationEntity.Author, sentNotification.Author);
            Assert.Equal(notificationEntity.ButtonTitle, sentNotification.ButtonTitle);
            Assert.Equal(notificationEntity.ButtonLink, sentNotification.ButtonLink);
            Assert.Equal(notificationEntity.SentDate, sentNotification.SentDate);
            Assert.Equal(notificationEntity.CreatedDate, sentNotification.CreatedDateTime);
            Assert.Equal(notificationEntity.Teams, sentNotification.TeamNames);
            Assert.Equal(notificationEntity.Rosters, sentNotification.RosterNames);
            Assert.Equal(notificationEntity.Groups, sentNotification.GroupNames);
            Assert.Equal(notificationEntity.AllUsers, sentNotification.AllUsers);
            Assert.Equal(notificationEntity.SendingStartedDate, sentNotification.SendingStartedDate);
            Assert.Equal(notificationEntity.ErrorMessage, sentNotification.ErrorMessage);
            Assert.Equal(notificationEntity.WarningMessage, sentNotification.WarningMessage);
        }

        private SentNotificationsController GetControllerInstance(bool ProactivelyInstallUserApp = true)
        {
            dataQueueMessageOptions.Setup(x => x.Value).Returns(new DataQueueMessageOptions() { ForceCompleteMessageDelayInSeconds = 100 });
            userAppOptions.Setup(x => x.Value).Returns(new UserAppOptions() { ProactivelyInstallUserApp = ProactivelyInstallUserApp, UserAppExternalId = "externalId" });
            Mock<ILogger<SentNotificationsController>> log = new Mock<ILogger<SentNotificationsController>>();
            loggerFactory.Setup(x => x.CreateLogger("SentNotificationsController")).Returns(log.Object);
            var controller = new SentNotificationsController(notificationDataRepository.Object, sentNotificationDataRepository.Object, teamDataRepository.Object, prepareToSendQueue.Object, dataQueue.Object, dataQueueMessageOptions.Object, groupsService.Object, exportDataRepository.Object, appCatalogService.Object, appSettingsService.Object, userAppOptions.Object, loggerFactory.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(Common.Constants.ClaimTypeUserId, "claimTypeUserId")

            }, "mock"));

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            return controller;
        }

        private NotificationDataEntity getNotification()
        {
            return new NotificationDataEntity()
            {
                Id = "id",
                Title = "titile",
                ImageLink = "imageLink",
                Summary = "summary",
                Author = "author",
                ButtonTitle = "title",
                ButtonLink = "buttonLink",
                SentDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Teams = new List<string>() { "item1", "item2" },
                Rosters = new List<string>() { "item1", "item2" },
                Groups = new List<string>() { "group1", "group2" },
                AllUsers = true,
                ErrorMessage = "errorMessage",
                WarningMessage = "warningMessage",
                Succeeded = 200,
                Failed = 1,
                TotalMessageCount = 1,
                SendingStartedDate = DateTime.Now,
                Status = "success",
                Unknown = 1,
                TeamsInString = "['item1','item2']",
                RostersInString = "['item1','item2']",
                GroupsInString = "['group1','group2']",
            };
        }
    }
}