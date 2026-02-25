# Handler Test Coverage

Bu dokümanda ApplicationService içindeki command/query handler'ların unit test durumu listelenir.

## Testi Olan Handler'lar

| Handler | Test Dosyası | Test Sayısı |
|---------|--------------|-------------|
| **Service** | | |
| DisableServiceByIdCommandHandler | Unit/Service/DisableServiceByIdCommandHandlerTests.cs | 4 |
| CreateServiceCommandHandler | Unit/Service/CreateServiceCommandHandlerTests.cs | 3 |
| UpdateServiceByIdCommandHandler | Unit/Service/UpdateServiceByIdCommandHandlerTests.cs | 3 |
| ReactivateOrExtendExpiryCommandHandler | Unit/Service/ReactivateOrExtendExpiryCommandHandlerTests.cs | 3 |
| GetServiceBySlugQueryHandler | Unit/Service/GetServiceBySlugQueryHandlerTests.cs | 2 |
| GetServicesByIdQueryHandler | Unit/Service/GetServicesByIdQueryHandlerTests.cs | 2 |
| GetServicesByFilterQueryHandler | Unit/Service/GetServicesByFilterQueryHandlerTests.cs | 2 |
| GetServicesBySearchTermQueryHandler | Unit/Service/GetServicesBySearchTermQueryHandlerTests.cs | 3 |
| GetServicesByRandomlyQueryHandler | Unit/Service/GetServicesByRandomlyQueryHandlerTests.cs | 2 |
| GetServicesBySellerIdQueryHandler | Unit/Service/GetServicesBySellerIdQueryHandlerTests.cs | 2 |
| DeleteServiceByIdCommandHandler | Unit/Service/DeleteServiceByIdCommandHandlerTests.cs | 1 |
| IncreaseServiceViewCountCommandHandler | Unit/Service/IncreaseServiceViewCountCommandHandlerTests.cs | 1 |
| **Category** | | |
| CreateCategoryCommandHandler | Unit/Category/CreateCategoryCommandHandlerTests.cs | 2 |
| UpdateCategoryCommandHandler | Unit/Category/UpdateCategoryCommandHandlerTests.cs | 2 |
| DeleteCategoryCommandHandler | Unit/Category/DeleteCategoryCommandHandlerTests.cs | 2 |
| CreateSubCategoryCommandHandler | Unit/Category/CreateSubCategoryCommandHandlerTests.cs | 6 |
| GetCategoriesQueryHandler | Unit/Category/GetCategoriesQueryHandlerTests.cs | 3 |
| GetCategoriesQuery2Handler | Unit/Category/GetCategoriesQuery2HandlerTests.cs | 3 |
| GetMainCategoriesQueryHandler | Unit/Category/GetMainCategoriesQueryHandlerTests.cs | 3 |
| **Seller** | | |
| CreateSellerCommandHandler | Unit/Seller/CreateSellerCommandHandlerTests.cs | 4 |
| GetSellerByIdQueryHandler | Unit/Seller/GetSellerByIdQueryHandlerTests.cs | 2 |
| GetSellerProfileQueryHandler | Unit/Seller/GetSellerProfileQueryHandlerTests.cs | 2 |
| GetSellerListQueryHandler | Unit/Seller/GetSellerListQueryHandlerTests.cs | 2 |
| GetSellerQueryHandler | Unit/Seller/GetSellerQueryHandlerTests.cs | 2 |
| UpdateSellerCommandHandler | Unit/Seller/UpdateSellerCommandHandlerTests.cs | 2 |
| UpdateSellerProfileCommandHandler | Unit/Seller/UpdateSellerProfileCommandHandlerTests.cs | 2 |
| **Admin** | | |
| ApproveListingCommandHandler | Unit/Admin/ApproveListingCommandHandlerTests.cs | 3 |
| SuspendListingCommandHandler | Unit/Admin/SuspendListingCommandHandlerTests.cs | 3 |
| SuspendSellerCommandHandler | Unit/Admin/SuspendSellerCommandHandlerTests.cs | 3 |
| GetAdminListingsQueryHandler | Unit/Admin/GetAdminListingsQueryHandlerTests.cs | 3 |
| GetAdminStatsQueryHandler | Unit/Admin/GetAdminStatsQueryHandlerTests.cs | 2 |
| **LegalDocument** | | |
| CreateLegalDocumentCommandHandler | Unit/LegalDocument/CreateLegalDocumentCommandHandlerTests.cs | 1 |
| GetLegalDocumentByIdQueryHandler | Unit/LegalDocument/GetLegalDocumentByIdQueryHandlerTests.cs | 2 |
| GetLegalDocumentByTypeQueryHandler | Unit/LegalDocument/GetLegalDocumentByTypeQueryHandlerTests.cs | 2 |
| GetActiveLegalDocumentsByTypesQueryHandler | Unit/LegalDocument/GetActiveLegalDocumentsByTypesQueryHandlerTests.cs | 3 |
| GetAllLegalDocumentsQueryHandler | Unit/LegalDocument/GetAllLegalDocumentsQueryHandlerTests.cs | 2 |
| UpdateLegalDocumentCommandHandler | Unit/LegalDocument/UpdateLegalDocumentCommandHandlerTests.cs | 2 |
| DeleteLegalDocumentCommandHandler | Unit/LegalDocument/DeleteLegalDocumentCommandHandlerTests.cs | 2 |
| **Favorite** | | |
| DeleteFavoriteCommandHandler | Unit/Favorite/DeleteFavoriteCommandHandlerTests.cs | 3 |
| AddFavoriteCommandHandler | Unit/Favorite/AddFavoriteCommandHandlerTests.cs | 2 |
| GetFavoriteListQueryHandler | Unit/Favorite/GetFavoriteListQueryHandlerTests.cs | 2 |
| GetFavoriteQueryHandler | Unit/Favorite/GetFavoriteQueryHandlerTests.cs | 2 |
| **SubscriptionPlan** | | |
| CreateSubscriptionPlanCommandHandler | Unit/SubscriptionPlan/CreateSubscriptionPlanCommandHandlerTests.cs | 1 |
| UpdateSubscriptionPlanCommandHandler | Unit/SubscriptionPlan/UpdateSubscriptionPlanCommandHandlerTests.cs | 2 |
| DeleteSubscriptionPlanCommandHandler | Unit/SubscriptionPlan/DeleteSubscriptionPlanCommandHandlerTests.cs | 4 |
| **SellerSubscription** | | |
| CreateSellerSubscriptionCommandHandler | Unit/SellerSubscription/CreateSellerSubscriptionCommandHandlerTests.cs | 4 |
| UpgradeSellerSubscriptionCommandHandler | Unit/SellerSubscription/UpgradeSellerSubscriptionCommandHandlerTests.cs | 4 |
| GetSellerSubscriptionQueryHandler | Unit/SellerSubscription/GetSellerSubscriptionQueryHandlerTests.cs | 2 |
| **Helpers** | | |
| SearchTermHelper | Unit/Helpers/SearchTermHelperTests.cs | 9 |
| **Message** | | |
| GetUnreadCountQueryHandler | Unit/Message/GetUnreadCountQueryHandlerTests.cs | 2 |
| GetConversationsQueryHandler | Unit/Message/GetConversationsQueryHandlerTests.cs | 1 |
| GetConversationMessagesQueryHandler | Unit/Message/GetConversationMessagesQueryHandlerTests.cs | 2 |
| SendMessageCommandHandler | Unit/Message/SendMessageCommandHandlerTests.cs | 3 |
| ReplyMessageCommandHandler | Unit/Message/ReplyMessageCommandHandlerTests.cs | 4 |
| MarkMessagesAsReadCommandHandler | Unit/Message/MarkMessagesAsReadCommandHandlerTests.cs | 2 |
| **SupportTicket** | | |
| CreateSupportTicketCommandHandler | Unit/SupportTicket/CreateSupportTicketCommandHandlerTests.cs | 5 |
| GetSupportTicketsQueryHandler | Unit/SupportTicket/GetSupportTicketsQueryHandlerTests.cs | 3 |
| UpdateSupportTicketCommandHandler | Unit/SupportTicket/UpdateSupportTicketCommandHandlerTests.cs | 2 |
| **UserLegalDocumentAcceptance** | | |
| SaveUserLegalDocumentAcceptancesCommandHandler | Unit/UserLegalDocumentAcceptance/SaveUserLegalDocumentAcceptancesCommandHandlerTests.cs | 4 |
| **Contract** | | |
| CreateContractCommandHandler | Unit/Contract/CreateContractCommandHandlerTests.cs | 1 |

## Henüz Testi Olmayan Handler'lar (eklenebilir) Handler'lar (eklenebilir)

- **Service:** (tüm listelenen handler'lar için test eklendi)
- **Category:** (tüm listelenen handler'lar için test eklendi)
- **Seller:** (tüm listelenen handler'lar için test eklendi)
- **Admin:** (tüm listelenen handler'lar için test eklendi)
- **LegalDocument:** (tüm listelenen handler'lar için test eklendi)
- **SellerSubscription:** (tüm listelenen handler'lar için test eklendi)
- **Favorite:** (tüm listelenen handler'lar için test eklendi)
- **Message / SupportTicket / UserLegalDocumentAcceptance / Contract:** (tüm listelenen handler'lar için test eklendi)

Test çalıştırma: `dotnet test MyIndustry.Tests/MyIndustry.Tests.csproj`
