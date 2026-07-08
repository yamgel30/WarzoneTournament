Imports AHAController.AHAEDM
Imports AHAController.AHAEDM.AHAForm2

<ServiceContract(Namespace:="http://aha/IAHAService")>
Public Interface IAHAService12

    <OperationContract()>
    Function Login(ByVal accountID As String, ByVal userID As String, ByVal password As String,
                   ByVal clientCert As String, ByVal previousLoginToken As String,
                   ByVal sourceIP As String, ByVal productID As String) As AUSAuthentication.LoginResponse

    <OperationContract()>
    Function GetAHAListPending(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal searchCriteria As SearchCriteria, ByVal sessionID As Long) As GetAHAListResponse

    <OperationContract()>
    Function GetAHAListPendingSpecialCover(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal searchCriteria As SearchCriteria) As GetAHAListResponse

    <OperationContract()>
    Function GetAHAListRejected(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal searchCriteria As SearchCriteria) As GetAHAListResponse

    <OperationContract()>
    Function GetAHAListSubmitted(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal searchCriteria As SearchCriteria) As GetAHAListResponse

    <OperationContract()>
    Function GetAHAListInProgress(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal searchCriteria As SearchCriteria) As GetAHAListResponse

    <OperationContract()>
    Function SubmitAHA(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItem) As AHASubmissionResponse

    <OperationContract()>
    Function SubmitAHAShort(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItemShort) As AHASubmissionShortResponse

    <OperationContract()>
    Function ResubmitAHA(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItem) As AHASubmissionResponse

    <OperationContract()>
    Function ResubmitAHAShort(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItemShort) As AHASubmissionShortResponse

    <OperationContract()>
    Function ResubmitAHAAddendum(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItem, ByVal isAddendum As Boolean) As AHASubmissionResponse

    <OperationContract()>
    Function ResubmitAHAAddendumShort(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItemShort, ByVal isAddendum As Boolean) As AHASubmissionShortResponse



    <OperationContract()>
    Function UpdateAHA(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItem) As AHASubmissionResponse

    <OperationContract()>
    Function UpdateAHAShort(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItemShort) As AHASubmissionShortResponse

    <OperationContract()>
    Function GetBillingOfRendering(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String,
                                   ByVal renderingNPI As String) As GetProviderListResponse

    <OperationContract()>
    Function GetMembersAHATemplate(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal memberIDList As List(Of String)) As GetMembersAHATemplateResponse

    <OperationContract()>
    Function GetAHATemplate(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal memberID As String, ByVal memberDxHistList As List(Of AHADxHxSelection)) As GetAHATemplateResponse

    <OperationContract()>
    Function GetAHAItem(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaID As Long) As GetAHAItemResponse

    <OperationContract()>
    Function GetAHAItemShort(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaID As Long) As GetAHAItemShortResponse

    <OperationContract()>
    Function PartialSaveAHA(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItem,
                            ByVal isNew As Boolean, ByVal pageNumber As Integer, ByVal validateAHA As Boolean) As AHASubmissionResponse

    <OperationContract()>
    Function AIPartialSaveAHA(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItem,
                            ByVal isNew As Boolean, ByVal pageNumber As Integer, ByVal validateAHA As Boolean) As AHASubmissionResponse

    <OperationContract()>
    Function PartialSaveAHAShort(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaForm As AHAFormItemShort,
                            ByVal isNew As Boolean, ByVal pageNumber As Integer, ByVal validateAHA As Boolean) As AHASubmissionShortResponse


    <OperationContract()>
    Function GetMemberActiveCondition(ByVal memberID As String, ByVal year As String) As GetMemberConditionResponse

    <OperationContract()>
    Function GetMemberSuspiciousCondition(ByVal memberID As String, ByVal year As String) As GetMemberConditionResponse

    <OperationContract()>
    Function GetClaimDiagnosticList(ByVal claimId As Long) As GetClaimDxListResponse

    <OperationContract()>
    Function GetProviderStatusLetterCSV(ByVal renderingNPI As String, ByVal claimYear As Integer, ByVal ipaName As String,
                                        ByVal billingNPI As String) As GetProviderStatusLetterCSVResponse

    <OperationContract()>
    Function LoginAHAHome(ByVal userId As String, ByVal password As String, ByVal ipAddress As String) As AHAAtHomeLoginResponse

    <OperationContract()>
    Function ChangeAHAAtHomePassword(ByVal userId As String, ByVal newPassword As String, ByVal ipAddress As String) As GenericResponse

    <OperationContract()>
    Function GetAHAReport(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String,
                          ByVal claimId As String, ByVal exportFormat As String, ByVal hasAddendum As Boolean,
                          ByVal changeColor As Boolean) As GetAHAReportBytesResponse

    <OperationContract(Name:="GetAHAReportAddendumOrder")>
    Function GetAHAReport(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String,
                          ByVal claimId As String, ByVal exportFormat As String, ByVal hasAddendum As Boolean,
                          ByVal changeColor As Boolean, ByVal addendumFirst As Boolean) As GetAHAReportBytesResponse

    <OperationContract()>
    Function SingleSignOn(ByVal portalID As String, ByVal token As String, ByVal mac As String, ByVal ip As String) As Models.SingleSignOnResponse

    <OperationContract()>
    Function GetProvidersInformation(ByVal renderingList As List(Of String)) As GetProviderInformationResponse

    <OperationContract()>
    Function GetProvidersInformationFromBilling(ByVal billingList As List(Of String), ByVal pcpNPI As String, ByVal AHAYear As Integer, ByVal ipaName As String) As GetProviderInformationResponse

    <OperationContract()>
    Function GetRenderingNPIOfMember(ByVal memberID As String, ByVal billingList As List(Of String)) As GenericResponse

    '<OperationContract()>
    'Function LoadBillingProvider() As List(Of String)

    <OperationContract()>
    Function VerifyEligibility(ByVal memberID As String, ByVal dateOfService As Date, ByVal renderingNPI As String, ByVal billingNPI As String, ByVal payerID As String) As MemberEligibilityResponse


    <OperationContract()>
    Function GetPendingClaim(ByVal memberID As String, ByVal renderingNPI As String, ByVal billingNPI As String, ByVal ahaYear As Integer, ByVal status As Integer, Optional ByVal isShort As Boolean = False) As GetPendingClaimResponse
    <OperationContract()>
    Function GetPendingClaim2023(ByVal memberID As String, ByVal renderingNPI As String, ByVal billingNPI As String, ByVal ahaYear As Integer, ByVal status As Integer, ByVal AtHome As Boolean, Optional ByVal isShort As Boolean = False) As GetPendingClaimResponse

    <OperationContract()>
    Function GetRejectNotes(ByVal claimId As Long) As GetRejectNotesResponse

    <OperationContract()>
    Function SaveAHADxHxSelection(ByVal claimID As Long, ByVal ahaDXHxSelectionList As List(Of AHADxHxSelection)) As SaveAHADxHxSelectionResponse

    <OperationContract()>
    Function GetAHADxHxSelection(ByVal claimID As Long) As GetAHADxHxSelectionResponse

    <OperationContract()>
    Function SaveAHASuspiciousCondDxHxSelection(ByVal claimID As Long, ahaSuspiciousCondDxHxSelectionList As List(Of Condition)) As SaveSuspiciousCondDxHxSelectionResponse

    <OperationContract()>
    Function SaveAHASuspiciousCondDxHxSelection_V2(ByVal claimID As Long, ahaSuspiciousCondDxHxSelectionList As List(Of Condition)) As SaveSuspiciousCondDxHxSelectionResponse

    <OperationContract()>
    Function GetMemberDxHistory(ByVal memberId As String) As GetMemberAHADxHistoryResponse

    <OperationContract>
    Function GetMemberSuspiciousConditionReport(ByVal memberId As String, ByVal memberName As String) As GetAHAReportBytesResponse

    <OperationContract>
    Function GetMemberClaimStatus(ByVal memberId As String, ByVal claimClass As Integer) As GetMemberClaimStatus

    <OperationContract>
    Function GetAddendumInformation(ByVal claimID As Long) As GetAddendumResponse

    <OperationContract>
    Function SaveAddendumProvider(ByVal addendumItem As Addendum, ByVal newClaimID As Long, ByVal claimIDToResubmit As Long) As AddendeumProviderSaveResponse

    <OperationContract>
    Function GetAddendumFormReport(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String,
                      ByVal claimId As String, ByVal exportFormat As String, ByVal changeColor As Boolean) As GetAddendumFormReportBytesResponse

    <OperationContract>
    Function CheckOrphanMemberHasAHA(ByVal memberID As String, ByVal isEdit As Boolean, ByVal isResubmit As Boolean, ByVal ahaYear As Integer, Optional ByVal atHome As Boolean = 0) As CheckOrphanMemberHasAHAResponse

    <OperationContract>
    Function GetMemberEmptyForm(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal memberID As String,
                                ByVal renderingNPI As String, ByVal billingNPI As String, ByVal ipaName As String, ByVal formYear As Integer) As GetAHAReportBytesResponse

    <OperationContract>
    Function GetAHAHeaderList(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal memberID As String,
                              ByVal renderingNPI As String, ByVal billingNPI As String, ByVal ipaName As String, ByVal formYear As Integer) As GetAHAHeaderResponse

    <OperationContract>
    Function GetAHAYearItem(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal ahaYear As Integer) As GetAHAYearItemResponse

    <OperationContract>
    Function GetMemberClinicalDataForm(ByVal accountID As String, ByVal loginToken As String, ByVal sourceIP As String, ByVal memberID As String,
                                ByVal renderingNPI As String, ByVal billingNPI As String, ByVal ipaName As String, ByVal formYear As Integer) As GetAHAReportBytesResponse

    <OperationContract>
    Function GetHistoryPresentIllnessOption(ByVal ahaPR As Integer, ByVal ahaFL As Integer, ByVal ghpAdult As Integer, ByVal ghpPediatric As Integer, ByVal isshort As Integer) As HistoryPresentIllnessOptionsResponse

    <OperationContract>
    Function GetICDLookup(ByVal searchText As String, serviceDate As DateTime, ahaYear As Integer) As GetICDLookupResponse

    <OperationContract>
    Function CheckMemberHasAHA(ByVal memberID As String, ByVal isEdit As Boolean, ByVal isResubmit As Boolean, ByVal ahaYear As Integer, atHome As Boolean) As GenericResponse

    <OperationContract>
    Function GetDxAndSuspisiousForm(ahaHeader As FormHeaderSection, ByVal repType As String, ByVal ahaYear As Int16) As GetAHAReportBytesResponse

    <OperationContract>
    Function SaveError(ByVal sessionId As String, ByVal message As String, ByVal trace As String, ByVal requestTime As Integer?, ByVal errorCode As String, ByVal className As String) As Integer

    <OperationContract>
    Function CreateSession(ByVal accountNo As String, ByVal token As String, ByVal appUser As String, ByVal loginID As String, ByVal portalID As String, ByVal ip As String) As Long

    <OperationContract>
    Function SaveDBDebugLog(ByVal logID As Long, ByVal sessionID As Long, ByVal processID As String, ByVal processDesc As String, ByVal completeDate As DateTime?, ByVal dataContent As String, ByVal statusID As Integer, ByVal message As String) As Long

    <OperationContract>
    Function ValidateConcurrencyID(claimID As Long, concurrencyID As Long) As Integer

    <OperationContract>
    Function VerifyMemberHasAHAForYear(ByVal memberNumber As String, ByVal year As Integer, ByVal renderingNPI As String, Optional ByVal claimClassTag As Integer = 1) As Boolean

    <OperationContract>
    Function LogAckowledgement(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean

    <OperationContract>
    Function LogFunctQuadMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean

    <OperationContract>
    Function GetLogFunctQuadMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean

    <OperationContract>
    Function LogInflammatoryPolyarthritisMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean

    <OperationContract>
    Function GetLogInflammatoryPolyarthritisMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean

    <OperationContract>
    Function GetLogAckowledgement(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean

    <OperationContract()>
    Function GetRenderingNPIOfMemberPRAI(ByVal memberID As String, ByVal billingList As List(Of String)) As GenericResponse

    <OperationContract()>
    Function GetAHAAndAtHomeSummary(claimClass As Short, claimClassTag As Short?, criteria As SearchCriteria) As List(Of SummaryCount)

    <OperationContract()>
    Function GetAHAListInProgress2021(accountID As String, loginToken As String, sourceIP As String, searchCriteria As SearchCriteria) As GetAHAListResponse

    <OperationContract()>
    Function VerifyMemberHasTHAForYear(MemberID As String, ClaimClass As Short, ClaimID As Long, AtHome As Boolean) As GenericResponse

    <OperationContract()>
    Function AHAVerifySubProjectIsActive(ProjectName As String, Year As Short) As BooleanResponse

    <OperationContract()>
    Function GetPRAIReport(renderingNPI As String, memberID As String, claimID As Long, claimClassTag As Short, year As String, format As String) As GetAHAReportBytesResponse

    <OperationContract()>
    Function VerifyIfFormExistsForDOS(model As FormHeaderSection, claimClass As Short, claimID As Long?) As BooleanResponse
    <OperationContract>
    Function VerifyMemberHasAHAForYearV2(ByVal memberNumber As String, ByVal year As Integer, ByVal renderingNPI As String, ByVal AtHome As Boolean, Optional ByVal claimClassTag As Integer = 1) As Boolean

    <OperationContract()>
    Function SaveInfo(model As AHAAIInfo) As Boolean

    <OperationContract()>
    Function SaveAudio(model As AHAAIInfo) As Boolean

    <OperationContract()>
    Function GetInfo(model As AHAAIInfo) As GetAHAAIInfoResponse

    <OperationContract()>
    Function GetAudio(model As AHAAIInfo) As GetAHAAIInfoResponse

End Interface
