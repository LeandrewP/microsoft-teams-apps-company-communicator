﻿// <copyright file="DraftNotificationsControllerTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.Extensions.Localization;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamData;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Resources;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MicrosoftGraph;
using Microsoft.Teams.Apps.CompanyCommunicator.Controllers;
using Microsoft.Teams.Apps.CompanyCommunicator.DraftNotificationPreview;
using Moq;
using System;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.Teams.Apps.CompanyCommunicator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.Collections.Generic;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net;

namespace Microsoft.Teams.Apps.CompanyCommunicator.Test.Controllers
{
    /// <summary>
    /// DraftNotificationsController test class.
    /// </summary>
    public class DraftNotificationsControllerTest
    {
        private readonly Mock<INotificationDataRepository> notificationDataRepository = new Mock<INotificationDataRepository>();
        private readonly Mock<ITeamDataRepository> teamDataRepository = new Mock<ITeamDataRepository>();
        private readonly Mock<IDraftNotificationPreviewService> draftNotificationPreviewService = new Mock<IDraftNotificationPreviewService>();
        private readonly Mock<IGroupsService> groupsService = new Mock<IGroupsService>();
        private readonly Mock<IAppSettingsService> appSettingsService = new Mock<IAppSettingsService>();
        private readonly Mock<IStringLocalizer<Strings>> localizer = new Mock<IStringLocalizer<Strings>>();
        private readonly string notificationId = "notificationId";


        /// <summary>
        /// Constructor test for all parameters.
        /// </summary>
        [Fact]
        public void CreateInstance_AllParameters_ShouldBeSuccess()
        {
            // Arrange
            Action action = () => new DraftNotificationsController(notificationDataRepository.Object, teamDataRepository.Object, draftNotificationPreviewService.Object, appSettingsService.Object, localizer.Object, groupsService.Object);

            // Act and Assert.
            action.Should().NotThrow();
        }

        /// <summary>
        /// Constructor test for null parameter.
        /// </summary> 
        [Fact]
        public void CreateInstance_NullParamter_ThrowsArgumentNullException()
        {
            // Arrange
            Action action1 = () => new DraftNotificationsController(null /*notificationDataRepository*/, teamDataRepository.Object, draftNotificationPreviewService.Object, appSettingsService.Object, localizer.Object, groupsService.Object);
            Action action2 = () => new DraftNotificationsController(notificationDataRepository.Object, null /*teamDataRepository*/, draftNotificationPreviewService.Object, appSettingsService.Object, localizer.Object, groupsService.Object);
            Action action3 = () => new DraftNotificationsController(notificationDataRepository.Object, teamDataRepository.Object, null /*draftNotificationPreviewService*/, appSettingsService.Object, localizer.Object, groupsService.Object);
            Action action4 = () => new DraftNotificationsController(notificationDataRepository.Object, teamDataRepository.Object, draftNotificationPreviewService.Object, null /*appSettingsService*/, localizer.Object, groupsService.Object);
            Action action5 = () => new DraftNotificationsController(notificationDataRepository.Object, teamDataRepository.Object, draftNotificationPreviewService.Object, appSettingsService.Object, null /*localizer*/, groupsService.Object);
            Action action6 = () => new DraftNotificationsController(notificationDataRepository.Object, teamDataRepository.Object, draftNotificationPreviewService.Object, appSettingsService.Object, localizer.Object, null/*groupsService*/);

            // Act and Assert.
            action1.Should().Throw<ArgumentNullException>("notificationDataRepository is null.");
            action2.Should().Throw<ArgumentNullException>("teamDataRepository is null.");
            action3.Should().Throw<ArgumentNullException>("draftNotificationPreviewService is null.");
            action4.Should().Throw<ArgumentNullException>("appSettingsService is null.");
            action5.Should().Throw<ArgumentNullException>("localizer is null.");
            action6.Should().Throw<ArgumentNullException>("groupsService is null.");
        }

        /// <summary>
        /// Test case to check draftNotification handles null parameter.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task createDraft_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetDraftNotificationsController();

            // Act
            Func<Task> task = async () => await controller.CreateDraftNotificationAsync(null /*notification*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("notification is null");
        }

        /// <summary>
        /// Test case to validates a draft notification if Team contains more than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_TeamSizeMorethan20_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Teams = this.GetItemsList(21), Groups = new List<string>() };

            string warning = "NumberOfTeamsExceededLimitWarningFormat";
            var localizedString = new LocalizedString(warning, warning);
            localizer.Setup(_ => _[warning]).Returns(localizedString);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Test case to validates a draft notification if Team contains less than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_TeamSizeLessThan20_ReturnsNotificationId()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Teams = new List<string>() { "item1", "item2" }, Groups = new List<string>() };
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);

            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.Equal(((ObjectResult)result.Result).Value, notificationId);
        }


        /// <summary>
        /// Test case to validates a draft notification if Roaster contains more than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_RoastersSizeMorethan20_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Rosters = this.GetItemsList(21), Groups = new List<string>() };

            string warning = "NumberOfRostersExceededLimitWarningFormat";
            var localizedString = new LocalizedString(warning, warning);
            localizer.Setup(_ => _[warning]).Returns(localizedString);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Test case to validates a draft notification if roaster contains 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_RoastersSize20_returnsNotificationId()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Rosters = GetItemsList(20), Groups = new List<string>() };
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);

            // Act 
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.Equal(((ObjectResult)result.Result).Value, notificationId);
        }

        /// <summary>
        /// Test case to validates a draft notification if Group contains more than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_GroupSizeMorethan20_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            DraftNotification notification = new DraftNotification() { Groups = this.GetItemsList(21) };

            string warning = "NumberOfGroupsExceededLimitWarningFormat";
            var localizedString = new LocalizedString(warning, warning);
            localizer.Setup(_ => _[warning]).Returns(localizedString);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Test case to validates a draft notification if group contains less than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_GroupSizeLessThan20_ReturnsNotificationId()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Groups = new List<string>() { "item1", "item2" } };
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.Equal(((ObjectResult)result.Result).Value, notificationId);
        }

        /// <summary>
        /// Test case to validates a draft notification if list has hidden membership group then return forbid.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_GrouplistHasHiddenMembership_ReturnsForbidResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(true);
            var notification = new DraftNotification() { Groups = new List<string>() };

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        /// <summary>
        /// Test case to validates a draft notification if Group list has no hidden membership group return draft Notification.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDraft_GrouplistHasNoHiddenMembership_ReturnsOkObjectResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            var notification = new DraftNotification() { Groups = new List<string>() };
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        /// <summary>
        /// Test to verify create draft notfication with valid data return ok result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Post_ValidData_ReturnsOkObjectResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            var notification = new DraftNotification() { Groups = new List<string>() };
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.CreateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        /// <summary>
        /// Test case to check dupliate draft Notification handles null parameter.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DuplicateDraft_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetDraftNotificationsController();

            // Act
            Func<Task> task = async () => await controller.DuplicateDraftNotificationAsync(null /*id*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("id is null");
        }

        /// <summary>
        /// Test case to duplicate notification for invalid data gives not found result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DuplicateNofitication_InvalidData_ReturnsNotFoundResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));
            var id = "id";
            // Act
            var result = await controller.DuplicateDraftNotificationAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Test case for Duplicate an existing draft notification and return Ok result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DuplicateDraft_WithExistingDraftData_ReturnOkResult()
        {
            var notificationDataEntity = new NotificationDataEntity();
            // Arrange
            var controller = GetDraftNotificationsController();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(notificationDataEntity);
            var notificationId = "notificationId";

            string duplicate = "DuplicateText";
            var localizedString = new LocalizedString(duplicate, duplicate);
            localizer.Setup(_ => _[duplicate, It.IsAny<string>()]).Returns(localizedString);

            notificationDataRepository.Setup(x => x.DuplicateDraftNotificationAsync(It.IsAny<NotificationDataEntity>(), It.IsAny<string>()));
            // Act
            var result = await controller.DuplicateDraftNotificationAsync(notificationId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Test case to check Update draft Notification handles null parameter.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetDraftNotificationsController();

            // Act
            Func<Task> task = async () => await controller.UpdateDraftNotificationAsync(null /*notification*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("notification is null");
        }

        /// <summary>
        /// Test case to validates a draft notification if list has hidden membership group then return forbid.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_HiddenMembershipGroup_ReturnsForbidResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(true);
            var notification = GetDraftNotification();

            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        /// <summary>
        /// Test case to validates a draft notification if Roaster contains more than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_TeamSizeMoreThan20_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Teams = this.GetItemsList(21), Groups = new List<string>() };

            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            string warning = "NumberOfTeamsExceededLimitWarningFormat";
            var localizedString = new LocalizedString(warning, warning);
            localizer.Setup(_ => _[warning]).Returns(localizedString);

            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        /// <summary>
        /// Test case to validates a draft notification if Team contains less than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_TeamSizeLessThan20_ReturnsNotificationId()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Teams = this.GetItemsList(10), Groups = new List<string>() };
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
           
            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Test case to validates a draft notification if Roaster contains more than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_RoastersSizeMorethan20_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Rosters = this.GetItemsList(21), Groups = new List<string>() };

            string warning = "NumberOfRostersExceededLimitWarningFormat";
            var localizedString = new LocalizedString(warning, warning);
            localizer.Setup(_ => _[warning]).Returns(localizedString);

            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Test case to validates a update draft notification if roaster contains less than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_RoastersSizeLessThan20_ReturnsNotificationId()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Rosters = GetItemsList(10), Groups = new List<string>() };
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<OkResult>(result);
        }

         /// <summary>
        /// Test case to validates a draft notification if Group contains more than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_GroupSizeMorethan20_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            DraftNotification notification = new DraftNotification() { Groups = this.GetItemsList(21) };

            string warning = "NumberOfGroupsExceededLimitWarningFormat";
            var localizedString = new LocalizedString(warning, warning);
            localizer.Setup(_ => _[warning]).Returns(localizedString);

            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Test case to validates a draft notification if group contains less than 20 items.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_GroupSizeLessThan20_ReturnsNotificationId()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = new DraftNotification() { Groups = new List<string>() { "item1", "item2" } };
            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
            
            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<OkResult>(result);
        }
        
        /// <summary>
        /// Test case to validates a update draft should return Ok result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_ValidData_ReturnsOkResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = GetDraftNotification();

            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Test case to validates a update draft should return Ok result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDraft_CreateOrUpdateAsync_ShouldInvokedOnce()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = GetDraftNotification();

            groupsService.Setup(x => x.ContainsHiddenMembershipAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);
            notificationDataRepository.Setup(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
            
            // Act
            var result = await controller.UpdateDraftNotificationAsync(notification);

            // Assert
            notificationDataRepository.Verify(x => x.CreateOrUpdateAsync(It.IsAny<NotificationDataEntity>()), Times.Once());
        }

        /// <summary>
        /// Test case to check delete draft Notification by id handles null parameter.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteDraft_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetDraftNotificationsController();

            // Act
            Func<Task> task = async () => await controller.DeleteDraftNotificationAsync(null /*id*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("id is null");
        }

        /// <summary>
        /// Test case to delete draft notification for invalid id return not found result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteDraft_ForInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = GetDraftNotification();

            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));
            // Act
            var result = await controller.DeleteDraftNotificationAsync(notificationId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Test case to delete draft notification for valid id return ok result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteDraft_ForValidId_ReturnsOkResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = GetDraftNotification();
            var notificationDataEntity = new NotificationDataEntity();

            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.DeleteAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
            // Act
            var result = await controller.DeleteDraftNotificationAsync(notificationId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Test case to delete draft notification for valid id should also invoke DeleteAsync method once.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Delete_CallDeleteAsync_ShouldGetInvokedOnce()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notification = GetDraftNotification();
            var notificationDataEntity = new NotificationDataEntity();

            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            notificationDataRepository.Setup(x => x.DeleteAsync(It.IsAny<NotificationDataEntity>())).Returns(Task.CompletedTask);
            // Act
            var result = await controller.DeleteDraftNotificationAsync(notificationId);

            // Assert
            notificationDataRepository.Verify(x => x.DeleteAsync(It.IsAny<NotificationDataEntity>()), Times.Once());
        }

        /// <summary>
        /// Test case to get all draft notification summary.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_AllDraftSummary_ReturnsDraftSummaryList()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notificationEntityList = new List<NotificationDataEntity>() { new NotificationDataEntity() { Id = "notificationId", Title = "notificationTitle" } };
            var notificationEntity = notificationEntityList.FirstOrDefault();
            notificationDataRepository.Setup(x => x.GetAllDraftNotificationsAsync()).ReturnsAsync(notificationEntityList);

            // Act
            var result = await controller.GetAllDraftNotificationsAsync();
            var allDraftNotificationSummary = (List<DraftNotificationSummary>)result.Value;

            // Assert
            Assert.IsType<List<DraftNotificationSummary>>(allDraftNotificationSummary);
        }

        /// <summary>
        /// Test case to check mapping of draft notification summary response.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_CorrectMapping_ReturnsDraftNotificationSummary()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notificationEntityList = new List<NotificationDataEntity>() { new NotificationDataEntity() { Id ="notificationId", Title = "notificationTitle"} };
            var notificationEntity = notificationEntityList.FirstOrDefault();
            notificationDataRepository.Setup(x => x.GetAllDraftNotificationsAsync()).ReturnsAsync(notificationEntityList);
            
            // Act
            var result = await controller.GetAllDraftNotificationsAsync();
            var draftNotificationSummaryList = (List<DraftNotificationSummary>)result.Value;
            var draftNotificationSummary = draftNotificationSummaryList.FirstOrDefault();

            // Assert
            Assert.Equal(draftNotificationSummary.Id, notificationEntity.Id);
            Assert.Equal(draftNotificationSummary.Title, notificationEntity.Title);
        }

        /// <summary>
        /// Test case to check get draft Notification by id handles null parameter.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDraft_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetDraftNotificationsController();

            // Act
            Func<Task> task = async () => await controller.GetDraftNotificationByIdAsync(null /*id*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("id is null");
        }

        /// <summary>
        /// Test to check a draft notification by Id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_DraftNotificationById_ReturnsOkResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notificationDataEntity = new NotificationDataEntity()
            {
                TeamsInString = "['team1','team2']",
                RostersInString = "['roster1','roster2']",
                GroupsInString = "['group1','group2']",
            };

            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            // Act
            var result = await controller.GetDraftNotificationByIdAsync(notificationId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        /// <summary>
        /// Test to check a draft notification by Id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDraft_WithEmptyGroupsInString_ReturnsOkResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notificationDataEntity = new NotificationDataEntity()
            {
                TeamsInString = "['team1','team2']",
                RostersInString = "['roster1','roster2']",
                GroupsInString = null,
            };

            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            // Act
            var result = await controller.GetDraftNotificationByIdAsync(notificationId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        /// <summary>
        /// Test to check a draft notification by Id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_InvalidInputId_ReturnsNotFoundResult()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));
            var id = "invalidId";

            // Act
            var result = await controller.GetDraftNotificationByIdAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Test case to check get draft Notification by id handles null parameter.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDraftSummaryConsent_nullParam_throwsArgumentNullException()
        {
            // Arrange
            var controller = GetDraftNotificationsController();

            // Act
            Func<Task> task = async () => await controller.GetDraftNotificationSummaryForConsentByIdAsync(null /*notificationId*/);

            // Assert
            await task.Should().ThrowAsync<ArgumentNullException>("notificationId is null");
        }

        /// <summary>
        /// Test to check a draft notification summary consent page for invalid data return not found.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDraftSummaryConsent_ForInvalidData_ReturnsNotFound()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));
            
            // Act
            var result = await controller.GetDraftNotificationSummaryForConsentByIdAsync(notificationId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Test to check a draft notification summary consent for val
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_CorrectMapping_ReturnsDraftNotificationSummaryForConsentList()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notificationDataEntity = new NotificationDataEntity()
            {
                TeamsInString = "['data1','data1']",
                RostersInString = "['data1','data1']",
                GroupsInString = "['group1','group2']",
                AllUsers = true
            };
            var groupList = new List<Group>() { new Group() { Id = "Id1", DisplayName = "group1"}, new Group() { Id = "Id2", DisplayName = "group2" } };
            var teams = new List<string>(){ "data1", "data1"};
            var rosters = new List<string>(){ "data1", "data1" };
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            groupsService.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(groupList.ToAsyncEnumerable());
            teamDataRepository.Setup(x => x.GetTeamNamesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(teams);
            teamDataRepository.Setup(x => x.GetTeamNamesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(rosters);

            // Act
            var result = await controller.GetDraftNotificationSummaryForConsentByIdAsync(notificationId);
            var draftNotificationSummaryConsent = (DraftNotificationSummaryForConsent)((ObjectResult)result.Result).Value;

            // Assert
            Assert.Equal(draftNotificationSummaryConsent.NotificationId, notificationId);
            Assert.Equal(draftNotificationSummaryConsent.TeamNames, teams);
            Assert.Equal(draftNotificationSummaryConsent.RosterNames, rosters);
            Assert.Equal(draftNotificationSummaryConsent.AllUsers, notificationDataEntity.AllUsers);
            Assert.Equal(draftNotificationSummaryConsent.GroupNames, notificationDataEntity.Groups);
        }

        /// <summary>
        /// Test to check a draft notification summary consent page for invalid data return not found.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDraftSummaryConsent_ForValidData_ReturnsDraftSummaryForConsent()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var notificationDataEntity = new NotificationDataEntity()
            {
                TeamsInString = "['team1','team2']",
                RostersInString = "['roster1','roster2']",
                GroupsInString = "['group1','group2']",
                AllUsers = true
            };
            var groupList = new List<Group>() { new Group() };
            var teams = new List<string>() { "team1", "team2" };
            var rosters = new List<string>() { "roster1", "roster2" };
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDataEntity);
            groupsService.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(groupList.ToAsyncEnumerable());
            teamDataRepository.Setup(x => x.GetTeamNamesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(teams);
            teamDataRepository.Setup(x => x.GetTeamNamesByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(rosters);

            // Act
            var result = await controller.GetDraftNotificationSummaryForConsentByIdAsync(notificationId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        /// <summary>
        /// Test case verify preview draft notification for invalid input returns badrequest.
        /// </summary>
        /// <param name="draftNotificationPreviewRequest"></param>
        /// <param name="draftNotificationId"></param>
        /// <param name="teamsTeamId"></param>
        /// <param name="teamsChannelId"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(DraftPreviewPrams))]
        public async Task PreviewDraft_InvalidInput_ReturnsBadRequest(DraftNotificationPreviewRequest draftNotificationPreviewRequest,string draftNotificationId, string teamsTeamId, string teamsChannelId)
        {
            // Arrange
            if (draftNotificationPreviewRequest != null)
            {
                draftNotificationPreviewRequest.DraftNotificationId = draftNotificationId;
                draftNotificationPreviewRequest.TeamsTeamId = teamsTeamId;
                draftNotificationPreviewRequest.TeamsChannelId = teamsChannelId;
            }
            var controller = GetDraftNotificationsController();

            // Act
            var result = await controller.PreviewDraftNotificationAsync(draftNotificationPreviewRequest);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        public static IEnumerable<object[]> DraftPreviewPrams
        {
            get
            {
                return new[]
                {
                    new object[] {null /*draftNotificationPreviewRequest*/, "draftNotificationId", "teamsTeamId", "teamsChannelId" },
                    new object[] {new DraftNotificationPreviewRequest(), null /*draftNotificationId*/, "teamsTeamId", "teamsChannelId" },
                    new object[] {new DraftNotificationPreviewRequest(), "draftNotificationId", null /*teamsTeamId*/, "teamsChannelId" },
                    new object[] {new DraftNotificationPreviewRequest(), "draftNotificationId", "teamsTeamId", null /*teamsChannelId*/ }
                };
            }
        }

        /// <summary>
        /// Test to check preview draft for invalid draftNotificationId.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PreviewDraft_InvalidNotificationId_ReturnsBadRequest()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var draftNotificationPreviewRequest = GetdraftNotificationPreviewRequest();
            notificationDataRepository.Setup(x=>x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(default(NotificationDataEntity)));

            // Act
            var result = await controller.PreviewDraftNotificationAsync(draftNotificationPreviewRequest);
            var errorMessage = ((ObjectResult)result).Value;

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, $"Notification {draftNotificationPreviewRequest.DraftNotificationId} not found.");
        }

        /// <summary>
        /// Test case for preview Draft notification for valid input returns status code 200 OK.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PreviewDraft_ValidInput_ReturnsStatusCode200OK()
        {
            // Arrange
            var controller = GetDraftNotificationsController();
            var draftNotificationPreviewRequest = GetdraftNotificationPreviewRequest();
            var notificationDatEntity = new NotificationDataEntity();
            var HttpStatusCodeOk = 200;
            notificationDataRepository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(notificationDatEntity);
            appSettingsService.Setup(x => x.GetServiceUrlAsync()).ReturnsAsync("ServiceUrl");
            draftNotificationPreviewService.Setup(x => x.SendPreview(It.IsAny<NotificationDataEntity>(), It.IsAny<TeamDataEntity>(), It.IsAny<string>())).ReturnsAsync(HttpStatusCode.OK);

            // Act
            var result = await controller.PreviewDraftNotificationAsync(draftNotificationPreviewRequest);
            var statusCode = ((StatusCodeResult)result).StatusCode;
            
            // Assert
            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(statusCode, HttpStatusCodeOk);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DraftNotificationsController"/> class.
        /// </summary>
        private DraftNotificationsController GetDraftNotificationsController()
        {
            notificationDataRepository.Setup(x => x.TableRowKeyGenerator).Returns(new TableRowKeyGenerator());
            notificationDataRepository.Setup(x => x.TableRowKeyGenerator.CreateNewKeyOrderingOldestToMostRecent()).Returns(notificationId);
            var controller = new DraftNotificationsController(notificationDataRepository.Object, teamDataRepository.Object, draftNotificationPreviewService.Object, appSettingsService.Object, localizer.Object, groupsService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            return controller;
        }


        private DraftNotification GetDraftNotification()
        {
            return new DraftNotification() { Groups = new List<string>() };
        }

        private List<string> GetItemsList(int itemCount)
        {
            var itemList = new List<string>();
            for(int item = 1;item<= itemCount; item ++)
            {
                itemList.Add("item" + item);
            }
            return itemList;
        }

        private DraftNotificationPreviewRequest GetdraftNotificationPreviewRequest()
        {
            return new DraftNotificationPreviewRequest()
            {
                DraftNotificationId = "draftNotificationId",
                TeamsChannelId = "teamsChannelId",
                TeamsTeamId = "teamsTeamId"
            };
        }
    }
}
