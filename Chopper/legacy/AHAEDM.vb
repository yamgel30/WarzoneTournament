#Disable Warning BC40056 ' Namespace or type specified in the Imports 'Microsoft.VisualStudio.TestTools.UnitTesting' doesn't contain any public member or cannot be found. Make sure the namespace or the type is defined and contains at least one public member. Make sure the imported element name doesn't use any aliases.
Imports System.Security.Policy
Imports Microsoft.VisualStudio.TestTools.UnitTesting
#Enable Warning BC40056 ' Namespace or type specified in the Imports 'Microsoft.VisualStudio.TestTools.UnitTesting' doesn't contain any public member or cannot be found. Make sure the namespace or the type is defined and contains at least one public member. Make sure the imported element name doesn't use any aliases.
Namespace AHAEDM

    Public Enum Errors
        'General Error Codes
        InternalError = -1
        Sucessfull = 0
        InvalidLoginToken = 1
        AccessDenied = 2
    End Enum

    <Serializable(), DataContract()>
    Public Class HistoryPresentIllnessOptionsResponse
        Inherits GenericResponse

        <DataMember()>
        Public Property HistoryPresentIllnessOptionList As List(Of AHAForm2.HistoryPresentIllnessOptions)

    End Class

    <Serializable(), DataContract()>
    Public Class GenericResponse
        Private _ErrorNumber As Integer
        Private _ReturnValue As String

        <DataMember()>
        Public Property ErrorNumber() As Integer
            Get
                Return _ErrorNumber
            End Get
            Set(ByVal value As Integer)
                _ErrorNumber = value
            End Set
        End Property

        <DataMember()>
        Public Overridable Property ReturnValue() As String
            Get
                Return _ReturnValue
            End Get
            Set(ByVal value As String)
                _ReturnValue = value
            End Set
        End Property
    End Class

    <Serializable(), DataContract()>
    Public Class BooleanResponse
        Inherits GenericResponse

        Private _Data As Boolean

        <DataMember()>
        Public Overridable Property Data() As Boolean
            Get
                Return _Data
            End Get
            Set(ByVal value As Boolean)
                _Data = value
            End Set
        End Property
    End Class

    <Serializable(), DataContract()>
    Public Class GetAddendumResponse
        Inherits GenericResponse

        Public Sub New()
            Addendum = New Addendum

        End Sub
        <DataMember()>
        Public Property Addendum As Addendum

    End Class

    <Serializable(), DataContract()>
    Public Class MedicationItem
        Public Sub New()
            MedicationName = ""
            isHistoric = False
            isConfirmed = False
            isFromAI = False
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub

        <DataMember()>
        Public Property MedicationName As String
        <DataMember()>
        Public Property isHistoric As Boolean
        <DataMember()>
        Public Property isConfirmed As Boolean
        <DataMember()>
        Public Property isFromAI As Boolean

    End Class


    <Serializable(), DataContract()>
    Public Class GetMemberClaimStatus
        Inherits GenericResponse

        <DataMember()>
        Public Property ClaimInfo As New ClaimItem

        <DataMember>
        Public Property CurrentStatus As EnumHelper.MemberClaimStatus

    End Class

    <Serializable(), DataContract()>
    Public Class ClaimItem

        <DataMember()>
        Public Property ClaimID As Long
        <DataMember()>
        Public Property Status As Integer

    End Class

    <Serializable(), DataContract()>
    Public Class SearchCriteria

        Public Sub New()
            RenderingNPIList = New List(Of String)
            BillingNPIList = New List(Of String)
        End Sub

        <DataMember()>
        Public Property RenderingNPIList As List(Of String)
        <DataMember()>
        Public Property BillingNPIList As List(Of String)
        <DataMember()>
        Public Property DateFrom As String
        <DataMember()>
        Public Property DateTo As String
        <DataMember()>
        Public Property MemberId As String
        <DataMember()>
        Public Property AHAYear As Integer
        <DataMember()>
        Public Property PayerID As String
        <DataMember()>
        Public Property AtHome As Boolean
        <DataMember()>
        Public Property MaxDayToResubmit As Integer
        <DataMember()>
        Public Property FormName As String
        <DataMember()>
        Public Property FormStatus As String
        <DataMember()>
        Public Property ClaimClass As Integer
        <DataMember()>
        Public Property PageSize As Integer
        <DataMember()>
        Public Property PageNumber As Integer
        <DataMember()>
        Public Property ClaimClassTag As Integer
    End Class

    <Serializable(), DataContract()>
    Public Class AHAAtHomeLoginResponse
        Inherits GenericResponse

        Private _AHAAtHomeUser As AHAAtHomeUserItem
        <DataMember()>
        Public Property AHAHomeUser() As AHAAtHomeUserItem
            Get
                Return _AHAAtHomeUser
            End Get
            Set(ByVal value As AHAAtHomeUserItem)
                _AHAAtHomeUser = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class AHAAtHomeUserItem

        Private _FullName As String
        <DataMember()>
        Public Property FullName() As String
            Get
                Return _FullName
            End Get
            Set(ByVal value As String)
                _FullName = value
            End Set
        End Property

        Private _RenderingNPI As String
        <DataMember()>
        Public Property RenderingNPI() As String
            Get
                Return _RenderingNPI
            End Get
            Set(ByVal value As String)
                _RenderingNPI = value
            End Set
        End Property

        Private _CreateDate As DateTime
        <DataMember()>
        Public Property CreateDate() As DateTime
            Get
                Return _CreateDate
            End Get
            Set(ByVal value As DateTime)
                _CreateDate = value
            End Set
        End Property

        Private _UpdateDate As DateTime
        <DataMember()>
        Public Property UpdateDate() As DateTime
            Get
                Return _UpdateDate
            End Get
            Set(ByVal value As DateTime)
                _UpdateDate = value
            End Set
        End Property

        Private _ChangePassword As Boolean
        <DataMember()>
        Public Property ChangePassword() As Boolean
            Get
                Return _ChangePassword
            End Get
            Set(ByVal value As Boolean)
                _ChangePassword = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class GetAHAListResponse
        Inherits GenericResponse

        Public Sub New()
            _AHAList = New List(Of AHAListItem)
        End Sub

        Private _AHAList As New List(Of AHAListItem)

        <DataMember()>
        Public Property AHAList() As List(Of AHAListItem)
            Get
                Return Me._AHAList
            End Get
            Set(ByVal value As List(Of AHAListItem))
                Me._AHAList = value
            End Set
        End Property

        <DataMember()>
        Public Property PageSize As Integer
        <DataMember()>
        Public Property PageNumber As Integer
        <DataMember()>
        Public Property RecordTotal As Integer

    End Class

    <Serializable(), DataContract()>
    Public Class GetProviderListResponse
        Inherits GenericResponse

        Private _ProviderList As New List(Of ProviderItem)

        <DataMember()>
        Public Property ProviderList() As List(Of ProviderItem)
            Get
                Return _ProviderList
            End Get
            Set(ByVal value As List(Of ProviderItem))
                _ProviderList = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class GetStringListResponse
        Inherits GenericResponse

        Private _ItemList As New List(Of String)

        <DataMember()>
        Public Property ItemList() As List(Of String)
            Get
                Return Me._ItemList
            End Get
            Set(ByVal value As List(Of String))
                Me._ItemList = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class GetMembersAHATemplateResponse
        Inherits GenericResponse

        Private _AHATemplateList As New List(Of AHAForm2.AHAFormItem)

        <DataMember()>
        Public Property AHATemplateList() As List(Of AHAForm2.AHAFormItem)
            Get
                Return _AHATemplateList
            End Get
            Set(ByVal value As List(Of AHAForm2.AHAFormItem))
                _AHATemplateList = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class GetAHATemplateResponse
        Inherits GenericResponse

        Private _AHATemplate As New AHAForm2.AHAFormItem

        <DataMember()>
        Public Property AHATemplate As AHAForm2.AHAFormItem
            Get
                Return _AHATemplate
            End Get
            Set(ByVal value As AHAForm2.AHAFormItem)
                _AHATemplate = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class GetClaimDxListResponse
        Inherits GenericResponse

        Private _DxList As New List(Of DiagnosisItem)
        <DataMember()>
        Public Property DiagnosticList() As List(Of DiagnosisItem)
            Get
                Return _DxList
            End Get
            Set(ByVal value As List(Of DiagnosisItem))
                _DxList = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class GetICDLookupResponse
        Inherits GenericResponse

        <DataMember()>
        Public Property ICDList As New List(Of DiagnosisItem)

    End Class


    '<Serializable(), DataContract()> _
    'Public Class GetMemberDataBatchResponse
    '    Inherits GenericResponse

    '    Private _MemberDataList As New List(Of MemberDataItem)

    '    <DataMember()> _
    '    Public Property MemberDataList() As List(Of MemberDataItem)
    '        Get
    '            Return _MemberDataList
    '        End Get
    '        Set(ByVal value As List(Of MemberDataItem))
    '            _MemberDataList = value
    '        End Set
    '    End Property

    'End Class

    <Serializable(), DataContract()>
    Public Class AHAListItem

        Private _ID As Nullable(Of Long)
        Private _SubmittedDate As Nullable(Of Date)
        Private _MemberID As String
        Private _MemberUID As String
        Private _MemberName As String
        Private _MemberBirthDate As Nullable(Of Date)
        Private _MemberGenderID As String
        Private _BillingNPI As String
        Private _RenderingNPI As String
        Private _PayerID As String
        Private _StatusCode As String
        Private _Source As String
        Private _Notes As String
        'Private _ClaimClassTag
        Private _ServiceDate As Nullable(Of Date)

        <DataMember()>
        Public Property ID() As Nullable(Of Long)
            Get
                Return Me._ID
            End Get
            Set(ByVal value As Nullable(Of Long))
                Me._ID = value
            End Set
        End Property

        <DataMember()>
        Public Property SubmittedDate() As Nullable(Of Date)
            Get
                Return Me._SubmittedDate
            End Get
            Set(ByVal value As Nullable(Of Date))
                Me._SubmittedDate = value
            End Set
        End Property

        <DataMember()>
        Public Property ServiceDate() As Nullable(Of Date)
            Get
                Return Me._ServiceDate
            End Get
            Set(ByVal value As Nullable(Of Date))
                Me._ServiceDate = value
            End Set
        End Property

        <DataMember()>
        Public Property MemberID() As String
            Get
                Return Me._MemberID
            End Get
            Set(ByVal value As String)
                Me._MemberID = value
            End Set
        End Property

        <DataMember()>
        Public Property MemberUID() As String
            Get
                Return Me._MemberUID
            End Get
            Set(ByVal value As String)
                Me._MemberUID = value
            End Set
        End Property

        <DataMember()>
        Public Property MemberName() As String
            Get
                Return Me._MemberName
            End Get
            Set(ByVal value As String)
                Me._MemberName = value
            End Set
        End Property

        <DataMember()>
        Public Property MemberBirthDate() As Nullable(Of Date)
            Get
                Return Me._MemberBirthDate
            End Get
            Set(ByVal value As Nullable(Of Date))
                Me._MemberBirthDate = value
            End Set
        End Property

        <DataMember()>
        Public Property MemberGenderID() As String
            Get
                Return Me._MemberGenderID
            End Get
            Set(ByVal value As String)
                Me._MemberGenderID = value
            End Set
        End Property

        <DataMember()>
        Public Property BillingNPI() As String
            Get
                Return Me._BillingNPI
            End Get
            Set(ByVal value As String)
                Me._BillingNPI = value
            End Set
        End Property

        <DataMember()>
        Public Property RenderingNPI() As String
            Get
                Return Me._RenderingNPI
            End Get
            Set(ByVal value As String)
                Me._RenderingNPI = value
            End Set
        End Property

        <DataMember()>
        Public Property PayerID() As String
            Get
                Return Me._PayerID
            End Get
            Set(ByVal value As String)
                Me._PayerID = value
            End Set
        End Property

        <DataMember()>
        Public Property StatusCode() As String
            Get
                Return Me._StatusCode
            End Get
            Set(ByVal value As String)
                Me._StatusCode = value
            End Set
        End Property

        <DataMember()>
        Public Property Source() As String
            Get
                Return Me._Source
            End Get
            Set(ByVal value As String)
                Me._Source = value
            End Set
        End Property

        <DataMember()>
        Public Property Notes() As String
            Get
                Return Me._Notes
            End Get
            Set(ByVal value As String)
                Me._Notes = value
            End Set
        End Property

        <DataMember()>
        Public Property ProviderName As String
        <DataMember()>
        Public Property ClaimClass As Integer
        <DataMember()>
        Public Property IsEditable As Boolean
        <DataMember()>
        Public Property PaymentStatus As String
        <DataMember()>
        Public Property DeniedCode As String
        <DataMember()>
        Public Property CheckNumber As String
        <DataMember()>
        Public Property CheckAmount As Decimal
        <DataMember()>
        Public Property CheckDate As Date
        <DataMember()>
        Public Property PaymentBillingNPI As String
        <DataMember()>
        Public Property RejectTypeID As Integer
        <DataMember()>
        Public Property IsPriority As Boolean
        <DataMember()>
        Public Property AtHome As Boolean
        <DataMember()>
        Public Property AddendumID As Long
        <DataMember()>
        Public Property HasAddendum As Boolean
        <DataMember()>
        Public Property DayLeft As String
        <DataMember()>
        Public Property StatusText As String
        <DataMember()>
        Public Property StatusTextToolTip As String
        <DataMember()>
        Public Property CanEdit As Boolean
        <DataMember()>
        Public Property CanPrint As Boolean
        <DataMember()>
        Public Property CanResubmit As Boolean
        <DataMember()>
        Public Property CanCreate As Boolean
        <DataMember()>
        Public Property CanViewRejectNotes As Boolean
        <DataMember()>
        Public Property ClaimClassTag As Integer
        <DataMember()>
        Public Property PriorityColor As String
        <DataMember()>
        Public Property PriorityLevel As Integer
    End Class

    <Serializable(), DataContract()>
    Public Class ProviderItem

        Private _BillingNPI As String
        Private _RenderingNPI As String
        Private _PayerID As String

        Public Sub New()
            BillingList = New List(Of DataBinding)
        End Sub

        <DataMember()>
        Public Property BillingNPI() As String
            Get
                Return _BillingNPI
            End Get
            Set(ByVal value As String)
                _BillingNPI = value
            End Set
        End Property

        <DataMember()>
        Public Property RenderingNPI() As String
            Get
                Return _RenderingNPI
            End Get
            Set(ByVal value As String)
                _RenderingNPI = value
            End Set
        End Property

        <DataMember()>
        Public Property PayerID() As String
            Get
                Return _PayerID
            End Get
            Set(ByVal value As String)
                _PayerID = value
            End Set
        End Property

        <DataMember()>
        Public Property DisplayText As String
        <DataMember()>
        Public Property BillingList As List(Of DataBinding)
        <DataMember()>
        Public Property RenderingName As String

    End Class

    <Serializable(), DataContract()>
    Public Class DataBinding

        <DataMember()>
        Public Property DataValue As String
        <DataMember()>
        Public Property DisplayText As String

    End Class

    <Serializable(), DataContract()>
    Public Class MemberDataItem

        Private _memberID As String
        Private _DiagnosisList As New List(Of String)
        Private _MedicationsList As New List(Of String)

        <DataMember()>
        Public Property MemberID() As String
            Get
                Return Me._memberID
            End Get
            Set(ByVal value As String)
                Me._memberID = value
            End Set
        End Property

        <DataMember()>
        Public Property DiagnosisList() As List(Of String)
            Get
                Return _DiagnosisList
            End Get
            Set(ByVal value As List(Of String))
                _DiagnosisList = value
            End Set
        End Property

        <DataMember()>
        Public Property MedicationsList() As List(Of String)
            Get
                Return _MedicationsList
            End Get
            Set(ByVal value As List(Of String))
                _MedicationsList = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class DiagnosisItem

        Private _Code As String
        <DataMember()>
        Public Property Code() As String
            Get
                Return _Code
            End Get
            Set(ByVal value As String)
                _Code = value
            End Set
        End Property

        Private _Description As String
        <DataMember()>
        Public Property Description() As String
            Get
                Return _Description
            End Get
            Set(ByVal value As String)
                _Description = value
            End Set
        End Property

        Private _Type As String
        <DataMember()>
        Public Property Type() As String
            Get
                Return _Type
            End Get
            Set(ByVal value As String)
                _Type = value
            End Set
        End Property

        Private _HCC_CMS As String
        <DataMember()>
        Public Property HCC_CMS() As String
            Get
                Return _HCC_CMS
            End Get
            Set(ByVal value As String)
                _HCC_CMS = value
            End Set
        End Property

        Private _HCC_Rx As String
        <DataMember()>
        Public Property HCC_Rx() As String
            Get
                Return _HCC_Rx
            End Get
            Set(ByVal value As String)
                _HCC_Rx = value
            End Set
        End Property

    End Class

#Region "Data Field Classes"

    <Serializable(), DataContract()>
    Public Class ErrorProperties

        'Private _SectionID As String
        '<DataMember()> _
        'Public Property SectionID() As String
        '    Get
        '        Return _SectionID
        '    End Get
        '    Set(ByVal value As String)
        '        _SectionID = value
        '    End Set
        'End Property

        Private _FieldID As String
        <DataMember()>
        Public Property FieldID() As String
            Get
                Return _FieldID
            End Get
            Set(ByVal value As String)
                _FieldID = value
            End Set
        End Property

        'Private _HasValue As Boolean = False
        '<DataMember()> _
        'Public Property HasValue() As Boolean
        '    Get
        '        Return _HasValue
        '    End Get
        '    Set(ByVal value As Boolean)
        '        _HasValue = value
        '    End Set
        'End Property

        Private _HasError As Boolean
        <DataMember()>
        Public Property HasError() As Boolean
            Get
                Return _HasError
            End Get
            Set(ByVal value As Boolean)
                _HasError = value
            End Set
        End Property

        Private _IsFromAI As Boolean
        <DataMember()>
        Public Property IsFromAI() As Boolean
            Get
                Return _IsFromAI
            End Get
            Set(ByVal value As Boolean)
                _IsFromAI = value
            End Set
        End Property

        Private _ErrorDesc As String
        <DataMember()>
        Public Property ErrorDescription() As String
            Get
                Return _ErrorDesc
            End Get
            Set(ByVal value As String)
                _ErrorDesc = value
            End Set
        End Property

        Private _ErrorCode As String
        <DataMember()>
        Public Property ErrorCode() As String
            Get
                Return _ErrorCode
            End Get
            Set(ByVal value As String)
                _ErrorCode = value
            End Set
        End Property

        Private _IsPreLoad As Boolean
        <DataMember()>
        Public Property IsPreLoad() As Boolean
            Get
                Return _IsPreLoad
            End Get
            Set(ByVal value As Boolean)
                _IsPreLoad = value
            End Set
        End Property

        Private _Tag As Object
        <DataMember()>
        Public Property Tag() As Object
            Get
                Return _Tag
            End Get
            Set(ByVal value As Object)
                _Tag = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class AHASectionErrorSummary

        Private _SectionID As String
        <DataMember()>
        Public Property SectionID() As String
            Get
                Return _SectionID
            End Get
            Set(ByVal value As String)
                _SectionID = value
            End Set
        End Property

        Private _ErrorFieldList As New List(Of ErrorProperties)
        <DataMember()>
        Public Property ErrorFieldList() As List(Of ErrorProperties)
            Get
                If _ErrorFieldList Is Nothing Then _ErrorFieldList = New List(Of ErrorProperties)

                Return _ErrorFieldList
            End Get
            Set(ByVal value As List(Of ErrorProperties))
                _ErrorFieldList = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class StringField
        Inherits ErrorProperties

        Private _Value As String
        <DataMember()>
        Public Property Value() As String
            Get
                Return _Value
            End Get
            Set(ByVal value As String)
                _Value = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class DateField
        Inherits ErrorProperties

        Private _Value As Nullable(Of Date)
        <DataMember()>
        Public Property Value() As Nullable(Of Date)
            Get
                Return _Value
            End Get
            Set(ByVal value As Nullable(Of Date))
                _Value = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class BooleanField
        Inherits ErrorProperties

        Public Sub New()
            'Me.Value = False
        End Sub

        Private _Value As Nullable(Of Boolean)
        <DataMember()>
        Public Property Value() As Nullable(Of Boolean)
            Get
                Return _Value
            End Get
            Set(ByVal value As Nullable(Of Boolean))
                _Value = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class IntegerField
        Inherits ErrorProperties

        Private _Value As Nullable(Of Integer)
        <DataMember()>
        Public Property Value() As Nullable(Of Integer)
            Get
                Return _Value
            End Get
            Set(ByVal value As Nullable(Of Integer))
                _Value = value
            End Set
        End Property

    End Class

    <Serializable(), DataContract()>
    Public Class DecimalField
        Inherits ErrorProperties

        Private _Value As Nullable(Of Decimal)
        <DataMember()>
        Public Property Value() As Nullable(Of Decimal)
            Get
                Return _Value
            End Get
            Set(ByVal value As Nullable(Of Decimal))
                _Value = value
            End Set
        End Property

    End Class


    <Serializable(), DataContract()>
    Public Class GenericFieldType(Of T)
        Inherits ErrorProperties

        Private _Value As T

        <DataMember()>
        Public Property Value() As T
            Get
                Return _Value
            End Get
            Set(ByVal value As T)
                _Value = value
            End Set
        End Property

    End Class


#End Region

    Namespace AHAForm2

        <Serializable(), DataContract()>
        Public Class AHAFormItem

            Public Sub New()

                Me.DiseasesOfTheSkin = New List(Of DiseasesOfTheSkin)
                Me.DxHistorySelectionList = New List(Of AHADxHxSelection)
            End Sub

            Private _FormSectionErrors As New List(Of AHASectionErrorSummary)
            <DataMember()>
            Public Property FormSectionErrors() As List(Of AHASectionErrorSummary)
                Get
                    Return _FormSectionErrors
                End Get
                Set(ByVal value As List(Of AHASectionErrorSummary))
                    _FormSectionErrors = value
                End Set
            End Property



            Private _ID As Nullable(Of Long)
            <DataMember()>
            Public Property ID() As Nullable(Of Long)
                Get
                    Return Me._ID
                End Get
                Set(ByVal value As Nullable(Of Long))
                    Me._ID = value
                End Set
            End Property

            Private _SubmittedDate As Nullable(Of Date)
            <DataMember()>
            Public Property SubmittedDate() As Nullable(Of Date)
                Get
                    Return Me._SubmittedDate
                End Get
                Set(ByVal value As Nullable(Of Date))
                    Me._SubmittedDate = value
                End Set
            End Property

            'Page 1
            Private _FormHeader As New FormHeaderSection
            <DataMember()>
            Public Property FormHeader() As FormHeaderSection
                Get
                    Return _FormHeader
                End Get
                Set(ByVal value As FormHeaderSection)
                    _FormHeader = value
                End Set
            End Property

            Private _ChiefComplaintPatientMedicalHistory As New ChiefComplaintPatientMedicalHistorySection
            <DataMember()>
            Public Property ChiefComplaintPatientMedicalHist() As ChiefComplaintPatientMedicalHistorySection
                Get
                    Return _ChiefComplaintPatientMedicalHistory
                End Get
                Set(ByVal value As ChiefComplaintPatientMedicalHistorySection)
                    _ChiefComplaintPatientMedicalHistory = value
                End Set
            End Property

            Private _AdvanceDirectives As New AdvanceDirectiveSection
            <DataMember()>
            Public Property AdvanceDirective() As AdvanceDirectiveSection
                Get
                    Return _AdvanceDirectives
                End Get
                Set(ByVal value As AdvanceDirectiveSection)
                    _AdvanceDirectives = value
                End Set
            End Property

            Private _ROS As New ReviewOfSystemSection
            <DataMember()>
            Public Property ReviewOfSystem() As ReviewOfSystemSection
                Get
                    Return _ROS
                End Get
                Set(ByVal value As ReviewOfSystemSection)
                    _ROS = value
                End Set
            End Property

            Private _MedicationList As New MedicationListSection
            <DataMember()>
            Public Property MedicationList() As MedicationListSection
                Get
                    Return _MedicationList
                End Get
                Set(ByVal value As MedicationListSection)
                    _MedicationList = value
                End Set
            End Property

            'Page 2
            Private _MedicalReview As New MedicalReviewSection
            <DataMember()>
            Public Property MedicalReview() As MedicalReviewSection
                Get
                    Return _MedicalReview
                End Get
                Set(ByVal value As MedicalReviewSection)
                    _MedicalReview = value
                End Set
            End Property

            Private _CognitiveAssesment As New CognitiveAssesmentSection
            <DataMember()>
            Public Property CognitiveAssesment() As CognitiveAssesmentSection
                Get
                    Return _CognitiveAssesment
                End Get
                Set(ByVal value As CognitiveAssesmentSection)
                    _CognitiveAssesment = value
                End Set
            End Property

            Private _PainScreening As New PainScreeningSection
            <DataMember()>
            Public Property PainScreening() As PainScreeningSection
                Get
                    Return _PainScreening
                End Get
                Set(ByVal value As PainScreeningSection)
                    _PainScreening = value
                End Set
            End Property

            Private _ActivitiesOfDailyLiving As New ActivitiesOfDailyLivingSection
            <DataMember()>
            Public Property ActivitiesOfDailyLiving() As ActivitiesOfDailyLivingSection
                Get
                    Return _ActivitiesOfDailyLiving
                End Get
                Set(ByVal value As ActivitiesOfDailyLivingSection)
                    _ActivitiesOfDailyLiving = value
                End Set
            End Property

            'Page 3
            Private _ScreeningSchedule As New ScreeningScheduleSection
            <DataMember()>
            Public Property ScreeningSchedule() As ScreeningScheduleSection
                Get
                    Return _ScreeningSchedule
                End Get
                Set(ByVal value As ScreeningScheduleSection)
                    _ScreeningSchedule = value
                End Set
            End Property

            Private _PhysicalExamination As New PhysicalExaminationSection
            <DataMember()>
            Public Property PhysicalExamination() As PhysicalExaminationSection
                Get
                    Return _PhysicalExamination
                End Get
                Set(ByVal value As PhysicalExaminationSection)
                    _PhysicalExamination = value
                End Set
            End Property

            'Page 4
            Private _AssessmentPlanOfTreatment As New AssessmentPlanOfTreatmentSection 'Diabetes
            <DataMember()>
            Public Property AssessmentPlanOfTreatment() As AssessmentPlanOfTreatmentSection
                Get
                    Return _AssessmentPlanOfTreatment
                End Get
                Set(ByVal value As AssessmentPlanOfTreatmentSection)
                    _AssessmentPlanOfTreatment = value
                End Set
            End Property

            Private _CancerDiagnosis As New List(Of CancerDiagnosisSection)
            <DataMember()>
            Public Property CancerDiagnosis() As List(Of CancerDiagnosisSection)
                Get
                    Return _CancerDiagnosis
                End Get
                Set(ByVal value As List(Of CancerDiagnosisSection))
                    _CancerDiagnosis = value
                End Set
            End Property

            <DataMember()>
            Public Property CancerDiagnosis_NA As Boolean

            Private _OtherCurrentConditions As New List(Of OtherCurrentConditionsSection)
            <DataMember()>
            Public Property OtherCurrentConditions() As List(Of OtherCurrentConditionsSection)
                Get
                    Return _OtherCurrentConditions
                End Get
                Set(ByVal value As List(Of OtherCurrentConditionsSection))
                    _OtherCurrentConditions = value
                End Set
            End Property

            Private _OtherCurrentConditionsAdditionalSection As New OtherCurrentConditionsAdditionalSection
            <DataMember()>
            Public Property OtherCurrentConditionsAdditional() As OtherCurrentConditionsAdditionalSection
                Get
                    Return _OtherCurrentConditionsAdditionalSection
                End Get
                Set(ByVal value As OtherCurrentConditionsAdditionalSection)
                    _OtherCurrentConditionsAdditionalSection = value
                End Set
            End Property

            Private _CKD As New ChronicKidneyDiseaseSection
            <DataMember()>
            Public Property CKD() As ChronicKidneyDiseaseSection
                Get
                    Return _CKD
                End Get
                Set(ByVal value As ChronicKidneyDiseaseSection)
                    _CKD = value
                End Set
            End Property

            Private _PressureSores As New PressureSoresSection
            <DataMember()>
            Public Property PressureSores() As PressureSoresSection
                Get
                    Return _PressureSores
                End Get
                Set(ByVal value As PressureSoresSection)
                    _PressureSores = value
                End Set
            End Property

            Private _RheumatoidArthritis As New RheumatoidArthritisSection
            <DataMember()>
            Public Property RheumatoidArthritis() As RheumatoidArthritisSection
                Get
                    Return _RheumatoidArthritis
                End Get
                Set(ByVal value As RheumatoidArthritisSection)
                    _RheumatoidArthritis = value
                End Set
            End Property

            Private _DepressionInventory As New DepressionInventorySection
            <DataMember()>
            Public Property DepressionInventory() As DepressionInventorySection
                Get
                    Return _DepressionInventory
                End Get
                Set(ByVal value As DepressionInventorySection)
                    _DepressionInventory = value
                End Set
            End Property

            Private _DMEUse As New DMEUseSection
            <DataMember()>
            Public Property DMEUse() As DMEUseSection
                Get
                    Return _DMEUse
                End Get
                Set(ByVal value As DMEUseSection)
                    _DMEUse = value
                End Set
            End Property

            Private _MyocardialInfarction As New MyocardialInfarctionSection
            <DataMember()>
            Public Property MyocardialInfarction() As MyocardialInfarctionSection
                Get
                    Return _MyocardialInfarction
                End Get
                Set(ByVal value As MyocardialInfarctionSection)
                    _MyocardialInfarction = value
                End Set
            End Property

            Private _BMIAssociatesDiagnosese As New BMIAssociatedDiagnosesSection
            <DataMember()>
            Public Property BMIAssociatedDiagnoses() As BMIAssociatedDiagnosesSection
                Get
                    Return _BMIAssociatesDiagnosese
                End Get
                Set(ByVal value As BMIAssociatedDiagnosesSection)
                    _BMIAssociatesDiagnosese = value
                End Set
            End Property

            Private _MajorDepression As New MajorDepressionSection
            <DataMember()>
            Public Property MajorDepression() As MajorDepressionSection
                Get
                    Return _MajorDepression
                End Get
                Set(ByVal value As MajorDepressionSection)
                    _MajorDepression = value
                End Set
            End Property

            Private _PressureSoresList As New List(Of PressureSoresSection)
            <DataMember()>
            Public Property PressureSoresList() As List(Of PressureSoresSection)
                Get
                    Return _PressureSoresList
                End Get
                Set(ByVal value As List(Of PressureSoresSection))
                    _PressureSoresList = value
                End Set
            End Property

            <DataMember()>
            Public Property DiseasesOfTheSkin As New List(Of DiseasesOfTheSkin)

            <DataMember()>
            Public Property DiseasesOfTheSking_NA As Boolean

            <DataMember()>
            Public Property CardiovascularDiseases As New CardiovascularDiseases

            <DataMember()>
            Public Property EyesAndNeurology As New EyesAndNeurology

            <DataMember()>
            Public Property DxHistorySelectionList As New List(Of AHADxHxSelection)

            <DataMember()>
            Public Property SuspiciousDxHxSelectionList As New List(Of Condition)

            <DataMember()>
            Public Property SessionID As Long

            <DataMember()>
            Public Property SocialDeterminants As New SocialDetermiants
            <DataMember()>
            Public Property PulmonaryDiseases As New PulmonaryDiseases


            <DataMember()>
            Public Property GastrointestinalDiseases As New GastrointestinalDiseases


            <DataMember()>
            Public Property ImLabRef As New ImLabRef

            <DataMember()>
            Public Property CongenitalDiseases As New CongenitalDiseases
            <DataMember()>
            Public Property Language As String

            <DataMember()>
            Public Property Gastrointestinal As New GastrointestinalSection

            <DataMember()>
            Public Property Musculoskeletal As New MusculoskeletalSection

            'Actualizacion 2020
            <DataMember()>
            Public Property MalnutritionCriteria As New MalnutritionCriteria
            <DataMember()>
            Public Property ScreeningSubstanceUseList As New List(Of ScreeningSubstanceUse)
            <DataMember()>
            Public Property SocialDeterminants2020 As New SocialDeterminants2020
            <DataMember()>
            Public Property ScreeningSubstanceUseListResult As New StringField
            <DataMember()>
            Public Property SocialDeterminants_NA As New BooleanField
            <DataMember()>
            Public Property SaveErrors As New List(Of Dictionary(Of String, String))
            <DataMember()>
            Public Property SocialDeterminants2023 As New SocialDeterminants2023
        End Class



        <Serializable, DataContract>
        Public Class AHAFormItemShort
            Inherits AHAFormItem

            Public Sub New()
                MyBase.New()
            End Sub

            Private Property _formHeaderSeccion As New FormHeaderShortSection
            Private Property _covidSecction As New CovidSection

            <DataMember>
            Public Property formHeaderSeccion() As FormHeaderShortSection
                Get
                    Return _formHeaderSeccion
                End Get
                Set(ByVal value As FormHeaderShortSection)
                    _formHeaderSeccion = value
                End Set
            End Property

            <DataMember>
            Public Property covidSecction() As CovidSection
                Get
                    Return _covidSecction
                End Get
                Set(ByVal value As CovidSection)
                    _covidSecction = value
                End Set
            End Property

        End Class



#Region " Form Header"

        <Serializable(), DataContract()>
        Public Class FormHeaderSection
            Inherits AHASectionErrorSummary

            Private _DateOfVisit As New DateField

            Private _MemberID As New StringField
            Private _MemberName As New StringField
            Private _MemberDOB As New DateField
            Private _MemberGender As New StringField

            Private _ProviderName As New StringField
            Private _BillingNPI As New StringField
            Private _RenderingNPI As New StringField
            Private _PayerID As New StringField

            Private _HealthPlan As New StringField
            Private _PlaceOfService As New IntegerField
            Private _Status As New IntegerField
            Private _AccompaniedBy As New StringField

            Private _isGHP As New BooleanField
            Private _isFL As New BooleanField
            Private _isPR As New BooleanField
            Private _ClaimClassTag As New IntegerField

            'Private _ConcurrencyID As New Long

            Public Sub New()
                Me.SectionID = "FH"
                Me.TypeOfVisit = -1
                Me.HealthPlan() = New StringField()
                Me.DateOfVisit() = New DateField()
                Me.MemberID() = New StringField()
                Me.MemberName() = New StringField()
                Me.MemberDOB() = New DateField()
                Me.MemberGender() = New StringField()
                Me.ProviderName() = New StringField()
                Me.BillingNPI() = New StringField()
                Me.RenderingNPI() = New StringField()
                Me.PayerID() = New StringField()
                Me.AtHome() = New BooleanField()
                Me.PlaceOfService() = New IntegerField()
                Me.Status() = New IntegerField()
                Me.ApprovedOrRejectedDate() = New DateField()
                Me.AccompaniedBy() = New StringField()
                Me.isGHP() = New BooleanField()
                Me.isPR() = New BooleanField()
                Me.isFl() = New BooleanField()
                Me.ClaimClassTag() = New IntegerField()
                Me.MemberFName = String.Empty
                Me.MemberMName = String.Empty
                Me.MemberLName = String.Empty
                Me.BillingName = String.Empty
                Me.RenderingName = String.Empty
                Me.IPAName = String.Empty
                Me.ProviderPostalCity = String.Empty
                Me.ProviderStreetCity = String.Empty
                Me.ConcurrencyID = 0
                Me.ProviderLocation = New StringField
                Me.PatientLocation = New StringField
                Me.Race = New StringField
                Me.Ethnicity = New StringField
                Me.Sexual_Orientation = New StringField()
                Me.Sex_Birth = New StringField()
                Me.Pronoun = New StringField()
                Me.Gender_Identity = New StringField()

                Me.Sexual_OrientationSomethingelse = New StringField()
                Me.PronounOther_Pronoun = New StringField()
                Me.Other_Race = New StringField()
                Me.Gender_Identity_AdditionalGender = New StringField()




                Me.Phone = New StringField
                Me.Email = New StringField
                Me.AdditionalHealthPlan = New StringField
                Me.AdditionalHealthPlanOther = New StringField
            End Sub

            <DataMember()>
            Public Property HealthPlan() As StringField
                Get
                    Return _HealthPlan
                End Get
                Set(ByVal value As StringField)
                    _HealthPlan = value
                End Set
            End Property

            <DataMember()>
            Public Property DateOfVisit() As DateField
                Get
                    Return _DateOfVisit
                End Get
                Set(ByVal value As DateField)
                    _DateOfVisit = value
                End Set
            End Property

            <DataMember()>
            Public Property MemberID() As StringField
                Get
                    Return _MemberID
                End Get
                Set(ByVal value As StringField)
                    _MemberID = value
                End Set
            End Property

            <DataMember()>
            Public Property MemberName() As StringField
                Get
                    Return _MemberName
                End Get
                Set(ByVal value As StringField)
                    _MemberName = value
                End Set
            End Property

            <DataMember()>
            Public Property MemberDOB() As DateField
                Get
                    Return _MemberDOB
                End Get
                Set(ByVal value As DateField)
                    _MemberDOB = value
                End Set
            End Property

            <DataMember()>
            Public Property MemberGender() As StringField
                Get
                    Return _MemberGender
                End Get
                Set(ByVal value As StringField)
                    _MemberGender = value
                End Set
            End Property

            <DataMember()>
            Public Property ProviderName() As StringField
                Get
                    Return _ProviderName
                End Get
                Set(ByVal value As StringField)
                    _ProviderName = value
                End Set
            End Property

            <DataMember()>
            Public Property BillingNPI() As StringField
                Get
                    Return _BillingNPI
                End Get
                Set(ByVal value As StringField)
                    _BillingNPI = value
                End Set
            End Property

            <DataMember()>
            Public Property RenderingNPI() As StringField
                Get
                    Return _RenderingNPI
                End Get
                Set(ByVal value As StringField)
                    _RenderingNPI = value
                End Set
            End Property

            <DataMember()>
            Public Property PayerID() As StringField
                Get
                    Return _PayerID
                End Get
                Set(ByVal value As StringField)
                    _PayerID = value
                End Set
            End Property

            Private _AtHome As New BooleanField

            <DataMember()>
            Public Property AtHome() As BooleanField
                Get
                    Return _AtHome
                End Get
                Set(ByVal value As BooleanField)
                    _AtHome = value
                End Set
            End Property

            <DataMember()>
            Public Property PlaceOfService() As IntegerField
                Get
                    Return _PlaceOfService
                End Get
                Set(ByVal value As IntegerField)
                    _PlaceOfService = value
                End Set
            End Property

            <DataMember()>
            Public Property Status() As IntegerField
                Get
                    Return _Status
                End Get
                Set(ByVal value As IntegerField)
                    _Status = value
                End Set
            End Property

            Private _ApprovedOrRejectedDate As New DateField
            <DataMember()>
            Public Property ApprovedOrRejectedDate() As DateField
                Get
                    Return _ApprovedOrRejectedDate
                End Get
                Set(ByVal value As DateField)
                    _ApprovedOrRejectedDate = value
                End Set
            End Property
            <DataMember()>
            Public Property AccompaniedBy() As StringField
                Get
                    Return _AccompaniedBy
                End Get
                Set(ByVal value As StringField)
                    _AccompaniedBy = value
                End Set
            End Property
            <DataMember()>
            Public Property isGHP() As BooleanField
                Get
                    Return _isGHP
                End Get
                Set(ByVal value As BooleanField)
                    _isGHP = value
                End Set
            End Property
            <DataMember()>
            Public Property isPR() As BooleanField
                Get
                    Return _isPR
                End Get
                Set(ByVal value As BooleanField)
                    _isPR = value
                End Set
            End Property
            <DataMember()>
            Public Property isFl() As BooleanField
                Get
                    Return _isFL
                End Get
                Set(ByVal value As BooleanField)
                    _isFL = value
                End Set
            End Property
            <DataMember()>
            Public Property ClaimClassTag() As IntegerField
                Get
                    Return _ClaimClassTag
                End Get
                Set(ByVal value As IntegerField)
                    _ClaimClassTag = value
                End Set
            End Property
            <DataMember()>
            Public Property MemberFName As String
            <DataMember()>
            Public Property MemberMName As String
            <DataMember()>
            Public Property MemberLName As String
            <DataMember()>
            Public Property BillingName As String
            <DataMember()>
            Public Property RenderingName As String
            <DataMember()>
            Public Property IPAName As String
            <DataMember()>
            Public Property ProviderPostalCity As String
            <DataMember()>
            Public Property ProviderStreetCity As String
            <DataMember()>
            Public Property TypeOfVisit As Integer
            <DataMember()>
            Public Property ConcurrencyID As Long
            <DataMember()>
            Public Property ProviderLocation As New StringField
            <DataMember()>
            Public Property PatientLocation As New StringField
            <DataMember()>
            Public Property Race As New StringField
            <DataMember()>
            Public Property Ethnicity As New StringField


            <DataMember()>
            Public Property Sexual_Orientation As New StringField()
            <DataMember()>
            Public Property Sex_Birth As New StringField()
            <DataMember()>
            Public Property Pronoun As New StringField()

            <DataMember()>
            Public Property Sexual_OrientationSomethingelse As New StringField()
            <DataMember()>
            Public Property PronounOther_Pronoun As New StringField()
            <DataMember()>
            Public Property Other_Race As New StringField()

            <DataMember()>
            Public Property Gender_Identity_AdditionalGender As New StringField()

            <DataMember()>
            Public Property Member_Lang As New StringField()

            <DataMember()>
            Public Property Gender_Identity As New StringField()

            <DataMember()>
            Public Property Phone As New StringField
            <DataMember()>
            Public Property AdditionalHealthPlan As New StringField
            <DataMember()>
            Public Property AdditionalHealthPlanOther As New StringField
            <DataMember()>
            Public Property Email As New StringField

            <DataMember()>
            Public Property Member_Lang_other As New StringField
        End Class

#End Region


#Region "Short Form Header"
        <Serializable(), DataContract()>
        Public Class FormHeaderShortSection
            Inherits FormHeaderSection
            '<DataMember>
            'Public Property ProviderLocation As New StringField

            '<DataMember>
            'Public Property PatientLocation As New StringField

        End Class
#End Region


#Region "Covid"
        <Serializable, DataContract>
        Public Class CovidSection
            Inherits AHASectionErrorSummary

            <DataMember>
            Public Property TripInTheLast14 As New BooleanField
            <DataMember>
            Public Property TripInTheLast14Description As New StringField
            <DataMember>
            Public Property CovidPositive As New BooleanField
            <DataMember>
            Public Property ContactCovidPositive As New BooleanField
            <DataMember>
            Public Property CovidNegative As New BooleanField
            <DataMember>
            Public Property OtherMedicalHistoryDescription As New StringField

        End Class

#End Region


#Region " Medication List"

        <Serializable(), DataContract()>
        Public Class MedicationListSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "MEDL"
                NotKnowAllergies = New BooleanField()
            End Sub

            Private _CurrentlyDoesNotUse As New BooleanField ' Nullable(Of Boolean)
            <DataMember()>
            Public Property CurrentlyDoesNotUse() As BooleanField ' Nullable(Of Boolean)
                Get
                    Return _CurrentlyDoesNotUse
                End Get
                Set(ByVal value As BooleanField) ' Nullable(Of Boolean))
                    _CurrentlyDoesNotUse = value
                End Set
            End Property

            Private _CurrentMedicationList As New List(Of MedicationItem)
            <DataMember()>
            Public Property CurrentMedication() As List(Of MedicationItem)
                Get
                    Return _CurrentMedicationList
                End Get
                Set(ByVal value As List(Of MedicationItem))
                    _CurrentMedicationList = value
                End Set
            End Property

            Private _AdherenceMedicationList As New List(Of MedicationItem)
            <DataMember()>
            Public Property AdherenceMedicationList() As List(Of MedicationItem)
                Get
                    Return _AdherenceMedicationList
                End Get
                Set(ByVal value As List(Of MedicationItem))
                    _AdherenceMedicationList = value
                End Set
            End Property

            <DataMember()>
            Public Property NotKnowAllergies As New BooleanField

            Private _AllergiesMedicationList As New List(Of MedicationItem)
            <DataMember()>
            Public Property AllergiesMedicationList() As List(Of MedicationItem)
                Get
                    Return _AllergiesMedicationList
                End Get
                Set(ByVal value As List(Of MedicationItem))
                    _AllergiesMedicationList = value
                End Set
            End Property

        End Class

#End Region

#Region " Advance Directives"

        <Serializable(), DataContract()>
        Public Class AdvanceDirectiveSection
            Inherits AHASectionErrorSummary

            Private _RefuseToCompleteAdvance As New BooleanField
            <DataMember()>
            Public Property RefuseToCompleteAdvance() As BooleanField
                Get
                    Return _RefuseToCompleteAdvance
                End Get
                Set(ByVal value As BooleanField)
                    _RefuseToCompleteAdvance = value
                End Set
            End Property

            Private _AdvanceCarePlanDiscussed As New BooleanField
            <DataMember()>
            Public Property AdvanceCarePlanDiscussed() As BooleanField
                Get
                    Return _AdvanceCarePlanDiscussed
                End Get
                Set(ByVal value As BooleanField)
                    _AdvanceCarePlanDiscussed = value
                End Set
            End Property

            Private _AdvanceCarePlanExecutedOn As New DateField
            <DataMember()>
            Public Property AdvanceCarePlanExecuteOn() As DateField
                Get
                    Return _AdvanceCarePlanExecutedOn
                End Get
                Set(ByVal value As DateField)
                    _AdvanceCarePlanExecutedOn = value
                End Set
            End Property

            Private _AdvanceCarePlanExecutedOnCheck As New BooleanField
            <DataMember()>
            Public Property AdvanceCarePlanExecutedOnCheck() As BooleanField
                Get
                    Return _AdvanceCarePlanExecutedOnCheck
                End Get
                Set(ByVal value As BooleanField)
                    _AdvanceCarePlanExecutedOnCheck = value
                End Set
            End Property

        End Class

#End Region

#Region " Chief Complaint & patient medical history"

        <Serializable(), DataContract()>
        Public Class ChiefComplaintPatientMedicalHistorySection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.HistoryOfPresentIllness() = New StringField()
                Me.RecentHospitalization() = New BooleanField()
                Me.RecentHospitalizationDate() = New DateField()
                Me.RecentSurgery() = New BooleanField()
                Me.RecentSurgeryDate() = New DateField()
                Me.AllergiesNotes() = New StringField()
                Me.NoAllergies() = New BooleanField()
                Me.TotalColectomy() = New BooleanField()
                Me.TotalColectomyDate() = New StringField()
                Me.BilateralMastectomy() = New BooleanField()
                Me.BilateralMastectomyDate() = New StringField()
                Me.UnilateralMastectomyRight() = New BooleanField()
                Me.UnilateralMastectomyRightDate() = New StringField()
                Me.UnilateralMastectomyLeft() = New BooleanField()
                Me.UnilateralMastectomyLeftDate() = New StringField()
                Me.NoSurgery() = New BooleanField()
                Me.OtherSurgery() = New StringField()
                Me.OtherSurgeryDate() = New StringField()
                Me.MedicalFamilySocialHistory() = New MedicalFamilySocialHistorySection()
                Me.ColectomyMastectomyNA = New BooleanField()
                Me.HistoryPresentIllnessSelectedText() = New StringField()
                Me.PatientConcentTelecomm() = New BooleanField()
                Me.ChoseDrinkingAlcohol = New BooleanField()
                Me.ChoseOtherSTD = New BooleanField()
                Me.ChoseQuittingTabacco = New BooleanField()
                Me.ChoseRiskofHIV = New BooleanField()
                Me.ChoseUseIllicitDrugs = New BooleanField()
            End Sub

            Private _HistOfPresentIllness As New StringField

            Private _RecentHosp As New BooleanField
            Private _RecentHospDate As New DateField
            'Private _RecentHospDueFracture As New BooleanField

            Private _RecentSurgery As New BooleanField
            Private _RecentSurgeryDate As New DateField

            Private _AllergiesNotes As New StringField
            Private _PatientConcentTelecomm As New BooleanField
            'Public Sub New()
            '    Me.HistoryPresentIllnessSelectedText.Value = ""
            'End Sub

            <DataMember()>
            Public Property HistoryOfPresentIllness() As StringField
                Get
                    Return _HistOfPresentIllness
                End Get
                Set(ByVal value As StringField)
                    _HistOfPresentIllness = value
                End Set
            End Property

            <DataMember()>
            Public Property RecentHospitalization() As BooleanField
                Get
                    Return _RecentHosp
                End Get
                Set(ByVal value As BooleanField)
                    _RecentHosp = value
                End Set
            End Property

            <DataMember()>
            Public Property RecentHospitalizationDate() As DateField
                Get
                    Return _RecentHospDate
                End Get
                Set(ByVal value As DateField)
                    _RecentHospDate = value
                End Set
            End Property

            '<DataMember()> _
            'Public Property RecentHospitalizationDueFracture() As BooleanField
            '    Get
            '        Return _RecentHospDueFracture
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _RecentHospDueFracture = value
            '    End Set
            'End Property

            <DataMember()>
            Public Property RecentSurgery() As BooleanField
                Get
                    Return _RecentSurgery
                End Get
                Set(ByVal value As BooleanField)
                    _RecentSurgery = value
                End Set
            End Property

            <DataMember()>
            Public Property RecentSurgeryDate() As DateField
                Get
                    Return _RecentSurgeryDate
                End Get
                Set(ByVal value As DateField)
                    _RecentSurgeryDate = value
                End Set
            End Property

            <DataMember()>
            Public Property AllergiesNotes() As StringField
                Get
                    Return _AllergiesNotes
                End Get
                Set(ByVal value As StringField)
                    _AllergiesNotes = value
                End Set
            End Property

            'Private _FamilySocialHistory As New StringField
            '<DataMember()> _
            'Public Property FamilySocialHistory() As StringField
            '    Get
            '        Return _FamilySocialHistory
            '    End Get
            '    Set(ByVal value As StringField)
            '        _FamilySocialHistory = value
            '    End Set
            'End Property

            Private _NoAllergies As New BooleanField
            <DataMember()>
            Public Property NoAllergies() As BooleanField
                Get
                    Return _NoAllergies
                End Get
                Set(ByVal value As BooleanField)
                    _NoAllergies = value
                End Set
            End Property

            Private _TotalColectomy As New BooleanField
            <DataMember()>
            Public Property TotalColectomy() As BooleanField
                Get
                    Return _TotalColectomy
                End Get
                Set(ByVal value As BooleanField)
                    _TotalColectomy = value
                End Set
            End Property

            Private _TotalColectomyDate As New StringField
            <DataMember()>
            Public Property TotalColectomyDate() As StringField
                Get
                    Return _TotalColectomyDate
                End Get
                Set(ByVal value As StringField)
                    _TotalColectomyDate = value
                End Set
            End Property

            Private _BilateralMastectomy As New BooleanField
            <DataMember()>
            Public Property BilateralMastectomy() As BooleanField
                Get
                    Return _BilateralMastectomy
                End Get
                Set(ByVal value As BooleanField)
                    _BilateralMastectomy = value
                End Set
            End Property

            Private _BilateralMastectomyDate As New StringField
            <DataMember()>
            Public Property BilateralMastectomyDate() As StringField
                Get
                    Return _BilateralMastectomyDate
                End Get
                Set(ByVal value As StringField)
                    _BilateralMastectomyDate = value
                End Set
            End Property

            Private _UnilateralMastectomyRight As New BooleanField
            <DataMember()>
            Public Property UnilateralMastectomyRight() As BooleanField
                Get
                    Return _UnilateralMastectomyRight
                End Get
                Set(ByVal value As BooleanField)
                    _UnilateralMastectomyRight = value
                End Set
            End Property

            Private _UnilateralMastectomyRightDate As New StringField
            <DataMember()>
            Public Property UnilateralMastectomyRightDate() As StringField
                Get
                    Return _UnilateralMastectomyRightDate
                End Get
                Set(ByVal value As StringField)
                    _UnilateralMastectomyRightDate = value
                End Set
            End Property

            Private _UnilateralMastectomyLeft As New BooleanField
            <DataMember()>
            Public Property UnilateralMastectomyLeft() As BooleanField
                Get
                    Return _UnilateralMastectomyLeft
                End Get
                Set(ByVal value As BooleanField)
                    _UnilateralMastectomyLeft = value
                End Set
            End Property

            Private _UnilateralMastectomyLeftDate As New StringField
            <DataMember()>
            Public Property UnilateralMastectomyLeftDate() As StringField
                Get
                    Return _UnilateralMastectomyLeftDate
                End Get
                Set(ByVal value As StringField)
                    _UnilateralMastectomyLeftDate = value
                End Set
            End Property

            Private _NoSurgery As New BooleanField
            <DataMember()>
            Public Property NoSurgery() As BooleanField
                Get
                    Return _NoSurgery
                End Get
                Set(ByVal value As BooleanField)
                    _NoSurgery = value
                End Set
            End Property

            Private _OtherSurgery As New StringField
            <DataMember()>
            Public Property OtherSurgery() As StringField
                Get
                    Return _OtherSurgery
                End Get
                Set(ByVal value As StringField)
                    _OtherSurgery = value
                End Set
            End Property

            Private _OtherSurgeryDate As New StringField
            <DataMember()>
            Public Property OtherSurgeryDate() As StringField
                Get
                    Return _OtherSurgeryDate
                End Get
                Set(ByVal value As StringField)
                    _OtherSurgeryDate = value
                End Set
            End Property

            Private _MedicalFamilySocialHistory As New MedicalFamilySocialHistorySection
            <DataMember()>
            Public Property MedicalFamilySocialHistory() As MedicalFamilySocialHistorySection
                Get
                    Return _MedicalFamilySocialHistory
                End Get
                Set(ByVal value As MedicalFamilySocialHistorySection)
                    _MedicalFamilySocialHistory = value
                End Set
            End Property

            <DataMember()>
            Public Property ColectomyMastectomyNA As BooleanField

            Private _HistoryPresentIllnessSelectedText As New StringField
            <DataMember()>
            Public Property HistoryPresentIllnessSelectedText() As StringField
                Get
                    Return _HistoryPresentIllnessSelectedText
                End Get
                Set(ByVal value As StringField)
                    _HistoryPresentIllnessSelectedText = value
                End Set
            End Property

            <DataMember()>
            Public Property PatientConcentTelecomm() As BooleanField
                Get
                    Return _PatientConcentTelecomm
                End Get
                Set(ByVal value As BooleanField)
                    _PatientConcentTelecomm = value
                End Set
            End Property


            <DataMember()>
            Public Property ChoseRiskofHIV As BooleanField
            <DataMember()>
            Public Property ChoseOtherSTD As BooleanField
            <DataMember()>
            Public Property ChoseQuittingTabacco As BooleanField
            <DataMember()>
            Public Property ChoseDrinkingAlcohol As BooleanField
            <DataMember()>
            Public Property ChoseUseIllicitDrugs As BooleanField
        End Class

#End Region

#Region " Medical Family Social History"

        <Serializable(), DataContract()>
        Public Class MedicalFamilySocialHistorySection
            Inherits AHASectionErrorSummary

            Public Sub New()

                Me.SectionID = "MFSH"

                Me.AlzheimerFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.AlzheimerMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.AlzheimerPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.AlzheimerSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.Anxiolytics = New BooleanField() With {.Value = False, .HasError = False}
                Me.CancerFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.CancerMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.CancerPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.CancerSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.Cannabis = New BooleanField() With {.Value = False, .HasError = False}
                Me.CholesterolFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.CholesterolMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.CholesterolPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.CholesterolSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.CounselAlcoholUse = New BooleanField() With {.HasError = False}
                Me.CounselIllicitDrugUse = New BooleanField() With {.HasError = False}
                Me.CounselTabaccoUse = New BooleanField() With {.HasError = False}
                Me.CVDFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.CVDMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.CVDPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.CVDSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.DMFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.DMMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.DMPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.DMSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.ErrorFieldList = New List(Of ErrorProperties)()
                Me.FatherNA = New BooleanField() With {.Value = False, .HasError = False}
                Me.HistoryAlcoholism = New BooleanField() With {.Value = False, .HasError = False}
                Me.HistoryCaffeineDependence = New BooleanField() With {.Value = False, .HasError = False}
                Me.HistoryDrugDependence = New BooleanField() With {.Value = False, .HasError = False}
                Me.Hypnotics = New BooleanField() With {.Value = False, .HasError = False}
                Me.MotherNA = New BooleanField() With {.Value = False, .HasError = False}
                Me.NA = New BooleanField() With {.Value = False, .HasError = False}
                Me.Nicotine = New BooleanField() With {.Value = False, .HasError = False}
                Me.Opiates = New BooleanField() With {.Value = False, .HasError = False}
                Me.OtherConditionText = New StringField
                Me.OtherDrugs = New BooleanField() With {.Value = False, .HasError = False}
                Me.OtherDrugsText = New StringField()
                Me.OtherFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.OtherMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.OtherPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.OtherSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.PatientNA = New BooleanField() With {.Value = False, .HasError = False}
                Me.RiskForHIV = New BooleanField() With {.HasError = False}
                Me.RiskForSTD = New BooleanField() With {.HasError = False}
                Me.Sedatives = New BooleanField() With {.Value = False, .HasError = False}
                Me.SibligsNA = New BooleanField() With {.Value = False, .HasError = False}
                Me.VIHFather = New BooleanField() With {.Value = False, .HasError = False}
                Me.VIHMother = New BooleanField() With {.Value = False, .HasError = False}
                Me.VIHPatient = New BooleanField() With {.Value = False, .HasError = False}
                Me.VIHSiblings = New BooleanField() With {.Value = False, .HasError = False}
                Me.HistoryOfMotherPregnancy = New StringField With {.Value = "", .HasError = False}
                Me.FisicalActivityScreening = New StringField With {.Value = "", .HasError = False}
                Me.FisicalActivityScreening_NA = New BooleanField() With {.Value = False, .HasError = False}
                Me.FisicalActivityScreening_DailyActivityRecommended = New BooleanField() With {.Value = False, .HasError = False}

                Me.NutricionalScreening_AdequateIntake = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_BalancedNutriciousDiet = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_Breastmilk = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_Cereal = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_CowMilk = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_FeedsItselft = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_Formula = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_JunkFood = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_Other = New StringField With {.Value = "", .HasError = False}
                Me.NutricionalScreening_Overweight = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_SodaJuices = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_SolidFoot = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_SupplementVitamins = New BooleanField() With {.HasError = False}
                Me.NutricionalScreening_Others_Checkbox = New BooleanField() With {.HasError = False}

                Me.DevelopmentHealth_NA = New BooleanField() With {.HasError = False}
                Me.DevelopmentScreening_CommunicationArea = New BooleanField() With {.HasError = False}
                Me.DevelopmentScreening_FineMotorSkillArea = New BooleanField() With {.HasError = False}
                Me.DevelopmentScreening_GrossMotorSkillsArea = New BooleanField() With {.HasError = False}
                Me.DevelopmentScreening_SocialIndividualSkillsArea = New BooleanField() With {.HasError = False}
                Me.DevelopmentScreening_ProblemResolutionSkillArea = New BooleanField() With {.HasError = False}
                Me.DevelopmentScreening_BehavioralHealthArea = New BooleanField() With {.HasError = False}

                Me.BehavioralHealth_NA = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_PhysicalMentalSelftRegulation = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_HabilityToFollowsInstructionsRules = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_SocialCommunication = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_AdaptativeFunctioning = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_Autonomy = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_CapacityToBeAffectiveEmpathic = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_InteractionWithPeople = New BooleanField() With {.HasError = False}
                Me.BehavioralHealth_UsesAlcoholDrugs = New BooleanField() With {.HasError = False}

                Me.AppropriateEducation_AppropiateUseCarSeat = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_BottleProp = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_PasiveSmoke = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_InfantCryingWhatToDo = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_ShakeBabyPrevention = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Firearm = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Pacifiers = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_ParentsReadToChild = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Emergency911 = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_FingerFoodChoking = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_DisciplinePrais = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_DrowningPrevention = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_NeverLeaveToddlerAlone = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_ToiletTraining = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_NutritionExercise = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_EstablishRoutineBedMealsToiletingEtc = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_UseSportProtection = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Bullying = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_OralHealth = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Others = New StringField() With {.Value = "", .HasError = False}
                Me.AppropriateEducation_SportInjuryPrevention = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_DrowningSunSafety = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_SafeAtHome = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_CorrectUseSeatbelt = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_SexualEducationSTD = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_DepresionAnxiety = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_RiskOfTattoosPiercing = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Autocontrol = New BooleanField() With {.HasError = False}
                Me.AppropriateEducation_Others_Checkbox = New BooleanField() With {.HasError = False}
            End Sub

            Private _NA As New BooleanField
            <DataMember()>
            Public Property NA() As BooleanField
                Get
                    Return _NA
                End Get
                Set(ByVal value As BooleanField)
                    _NA = value
                End Set
            End Property

            Private _PatientNA As New BooleanField
            <DataMember()>
            Public Property PatientNA() As BooleanField
                Get
                    Return _PatientNA
                End Get
                Set(ByVal value As BooleanField)
                    _PatientNA = value
                End Set
            End Property

            Private _MotherNA As New BooleanField
            <DataMember()>
            Public Property MotherNA() As BooleanField
                Get
                    Return _MotherNA
                End Get
                Set(ByVal value As BooleanField)
                    _MotherNA = value
                End Set
            End Property

            Private _FatherNA As New BooleanField
            <DataMember()>
            Public Property FatherNA() As BooleanField
                Get
                    Return _FatherNA
                End Get
                Set(ByVal value As BooleanField)
                    _FatherNA = value
                End Set
            End Property

            Private _SibligsNA As New BooleanField
            <DataMember()>
            Public Property SibligsNA() As BooleanField
                Get
                    Return _SibligsNA
                End Get
                Set(ByVal value As BooleanField)
                    _SibligsNA = value
                End Set
            End Property

            Private _DMPatient As New BooleanField
            <DataMember()>
            Public Property DMPatient() As BooleanField
                Get
                    Return _DMPatient
                End Get
                Set(ByVal value As BooleanField)
                    _DMPatient = value
                End Set
            End Property

            Private _DMMother As New BooleanField
            <DataMember()>
            Public Property DMMother() As BooleanField
                Get
                    Return _DMMother
                End Get
                Set(ByVal value As BooleanField)
                    _DMMother = value
                End Set
            End Property

            Private _DMFather As New BooleanField
            <DataMember()>
            Public Property DMFather() As BooleanField
                Get
                    Return _DMFather
                End Get
                Set(ByVal value As BooleanField)
                    _DMFather = value
                End Set
            End Property

            Private _DMSiblings As New BooleanField
            <DataMember()>
            Public Property DMSiblings() As BooleanField
                Get
                    Return _DMSiblings
                End Get
                Set(ByVal value As BooleanField)
                    _DMSiblings = value
                End Set
            End Property

            Private _CVDPatient As New BooleanField
            <DataMember()>
            Public Property CVDPatient() As BooleanField
                Get
                    Return _CVDPatient
                End Get
                Set(ByVal value As BooleanField)
                    _CVDPatient = value
                End Set
            End Property

            Private _CVDMother As New BooleanField
            <DataMember()>
            Public Property CVDMother() As BooleanField
                Get
                    Return _CVDMother
                End Get
                Set(ByVal value As BooleanField)
                    _CVDMother = value
                End Set
            End Property

            Private _CVDFather As New BooleanField
            <DataMember()>
            Public Property CVDFather() As BooleanField
                Get
                    Return _CVDFather
                End Get
                Set(ByVal value As BooleanField)
                    _CVDFather = value
                End Set
            End Property

            Private _CVDSiblings As New BooleanField
            <DataMember()>
            Public Property CVDSiblings() As BooleanField
                Get
                    Return _CVDSiblings
                End Get
                Set(ByVal value As BooleanField)
                    _CVDSiblings = value
                End Set
            End Property

            Private _CholesterolPatient As New BooleanField
            <DataMember()>
            Public Property CholesterolPatient() As BooleanField
                Get
                    Return _CholesterolPatient
                End Get
                Set(ByVal value As BooleanField)
                    _CholesterolPatient = value
                End Set
            End Property

            Private _CholesterolMother As New BooleanField
            <DataMember()>
            Public Property CholesterolMother() As BooleanField
                Get
                    Return _CholesterolMother
                End Get
                Set(ByVal value As BooleanField)
                    _CholesterolMother = value
                End Set
            End Property

            Private _CholesterolFather As New BooleanField
            <DataMember()>
            Public Property CholesterolFather() As BooleanField
                Get
                    Return _CholesterolFather
                End Get
                Set(ByVal value As BooleanField)
                    _CholesterolFather = value
                End Set
            End Property

            Private _CholesterolSiblings As New BooleanField
            <DataMember()>
            Public Property CholesterolSiblings() As BooleanField
                Get
                    Return _CholesterolSiblings
                End Get
                Set(ByVal value As BooleanField)
                    _CholesterolSiblings = value
                End Set
            End Property

            Private _CancerPatient As New BooleanField
            <DataMember()>
            Public Property CancerPatient() As BooleanField
                Get
                    Return _CancerPatient
                End Get
                Set(ByVal value As BooleanField)
                    _CancerPatient = value
                End Set
            End Property

            Private _CancerMother As New BooleanField
            <DataMember()>
            Public Property CancerMother() As BooleanField
                Get
                    Return _CancerMother
                End Get
                Set(ByVal value As BooleanField)
                    _CancerMother = value
                End Set
            End Property

            Private _CancerFather As New BooleanField
            <DataMember()>
            Public Property CancerFather() As BooleanField
                Get
                    Return _CancerFather
                End Get
                Set(ByVal value As BooleanField)
                    _CancerFather = value
                End Set
            End Property

            Private _CancerSiblings As New BooleanField
            <DataMember()>
            Public Property CancerSiblings() As BooleanField
                Get
                    Return _CancerSiblings
                End Get
                Set(ByVal value As BooleanField)
                    _CancerSiblings = value
                End Set
            End Property

            Private _AlzheimerPatient As New BooleanField
            <DataMember()>
            Public Property AlzheimerPatient() As BooleanField
                Get
                    Return _AlzheimerPatient
                End Get
                Set(ByVal value As BooleanField)
                    _AlzheimerPatient = value
                End Set
            End Property

            Private _AlzheimerMother As New BooleanField
            <DataMember()>
            Public Property AlzheimerMother() As BooleanField
                Get
                    Return _AlzheimerMother
                End Get
                Set(ByVal value As BooleanField)
                    _AlzheimerMother = value
                End Set
            End Property

            Private _AlzheimerFather As New BooleanField
            <DataMember()>
            Public Property AlzheimerFather() As BooleanField
                Get
                    Return _AlzheimerFather
                End Get
                Set(ByVal value As BooleanField)
                    _AlzheimerFather = value
                End Set
            End Property

            Private _AlzheimerSiblings As New BooleanField
            <DataMember()>
            Public Property AlzheimerSiblings() As BooleanField
                Get
                    Return _AlzheimerSiblings
                End Get
                Set(ByVal value As BooleanField)
                    _AlzheimerSiblings = value
                End Set
            End Property

            <DataMember()>
            Public Property VIHPatient As New BooleanField
            <DataMember()>
            Public Property VIHMother As New BooleanField
            <DataMember()>
            Public Property VIHFather As New BooleanField
            <DataMember()>
            Public Property VIHSiblings As New BooleanField


            Private _RiskForHIV As New BooleanField
            <DataMember()>
            Public Property RiskForHIV() As BooleanField
                Get
                    Return _RiskForHIV
                End Get
                Set(ByVal value As BooleanField)
                    _RiskForHIV = value
                End Set
            End Property

            Private _RiskForSTD As New BooleanField
            <DataMember()>
            Public Property RiskForSTD() As BooleanField
                Get
                    Return _RiskForSTD
                End Get
                Set(ByVal value As BooleanField)
                    _RiskForSTD = value
                End Set
            End Property

            Private _CounselTabaccoUse As New BooleanField
            <DataMember()>
            Public Property CounselTabaccoUse() As BooleanField
                Get
                    Return _CounselTabaccoUse
                End Get
                Set(ByVal value As BooleanField)
                    _CounselTabaccoUse = value
                End Set
            End Property

            Private _CounselIllicitDrugUse As New BooleanField
            <DataMember()>
            Public Property CounselIllicitDrugUse() As BooleanField
                Get
                    Return _CounselIllicitDrugUse
                End Get
                Set(ByVal value As BooleanField)
                    _CounselIllicitDrugUse = value
                End Set
            End Property

            Private _CounselAlcoholUse As New BooleanField
            <DataMember()>
            Public Property CounselAlcoholUse() As BooleanField
                Get
                    Return _CounselAlcoholUse
                End Get
                Set(ByVal value As BooleanField)
                    _CounselAlcoholUse = value
                End Set
            End Property

            Private _HistoryAlcoholism As New BooleanField
            <DataMember()>
            Public Property HistoryAlcoholism() As BooleanField
                Get
                    Return _HistoryAlcoholism
                End Get
                Set(ByVal value As BooleanField)
                    _HistoryAlcoholism = value
                End Set
            End Property

            Private _HistoryDrugDependence As New BooleanField
            <DataMember()>
            Public Property HistoryDrugDependence() As BooleanField
                Get
                    Return _HistoryDrugDependence
                End Get
                Set(ByVal value As BooleanField)
                    _HistoryDrugDependence = value
                End Set
            End Property

            <DataMember()>
            Public Property Nicotine As New BooleanField

            <DataMember()>
            Public Property Opiates As New BooleanField

            <DataMember()>
            Public Property Cannabis As New BooleanField

            <DataMember()>
            Public Property Sedatives As New BooleanField

            <DataMember()>
            Public Property Hypnotics As New BooleanField

            <DataMember()>
            Public Property Anxiolytics As New BooleanField

            <DataMember()>
            Public Property OtherDrugs As New BooleanField

            <DataMember()>
            Public Property OtherDrugsText As New StringField

            <DataMember()>
            Public Property HistoryCaffeineDependence As New BooleanField

            <DataMember()>
            Public Property OtherConditionText As New StringField

            <DataMember()>
            Public Property OtherPatient As New BooleanField

            <DataMember()>
            Public Property OtherMother As New BooleanField

            <DataMember()>
            Public Property OtherFather As New BooleanField

            <DataMember()>
            Public Property OtherSiblings As New BooleanField

            <DataMember()>
            Public Property HistoryOfMotherPregnancy As New StringField

            Private _FisicalActivityScreening As New StringField
            <DataMember()>
            Public Property FisicalActivityScreening() As StringField
                Get
                    Return _FisicalActivityScreening
                End Get
                Set(ByVal value As StringField)
                    _FisicalActivityScreening = value
                End Set
            End Property

            <DataMember()>
            Public Property FisicalActivityScreening_NA As New BooleanField

            <DataMember()>
            Public Property FisicalActivityScreening_DailyActivityRecommended As New BooleanField

            <DataMember()>
            Public Property NutricionalScreening_NA As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_Other As New StringField

            <DataMember()>
            Public Property NutricionalScreening_Breastmilk As New BooleanField

            <DataMember()>
            Public Property NutricionalScreening_Formula As New BooleanField

            <DataMember()>
            Public Property NutricionalScreening_AdequateIntake As New BooleanField

            <DataMember()>
            Public Property NutricionalScreening_Cereal As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_SolidFoot As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_CowMilk As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_SupplementVitamins As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_FeedsItselft As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_SodaJuices As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_BalancedNutriciousDiet As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_JunkFood As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_Overweight As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_UnderWeight As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_FoodAllergies As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_SpecialDiets As New BooleanField
            <DataMember()>
            Public Property NutricionalScreening_Others_Checkbox As New BooleanField

            <DataMember()>
            Public Property DevelopmentHealth_NA As New BooleanField

            <DataMember()>
            Public Property DevelopmentScreening_CommunicationArea As New BooleanField
            <DataMember()>
            Public Property DevelopmentScreening_FineMotorSkillArea As New BooleanField
            <DataMember()>
            Public Property DevelopmentScreening_GrossMotorSkillsArea As New BooleanField
            <DataMember()>
            Public Property DevelopmentScreening_SocialIndividualSkillsArea As New BooleanField
            <DataMember()>
            Public Property DevelopmentScreening_ProblemResolutionSkillArea As New BooleanField
            <DataMember()>
            Public Property DevelopmentScreening_BehavioralHealthArea As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_NA As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_PhysicalMentalSelftRegulation As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_HabilityToFollowsInstructionsRules As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_SocialCommunication As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_AdaptativeFunctioning As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_Autonomy As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_CapacityToBeAffectiveEmpathic As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_InteractionWithPeople As New BooleanField
            <DataMember()>
            Public Property BehavioralHealth_UsesAlcoholDrugs As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_NA As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_AppropiateUseCarSeat As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_BottleProp As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_PasiveSmoke As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_InfantCryingWhatToDo As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_ShakeBabyPrevention As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Firearm As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Pacifiers As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_ParentsReadToChild As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Emergency911 As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_FingerFoodChoking As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_DisciplinePrais As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_DrowningPrevention As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_NeverLeaveToddlerAlone As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_ToiletTraining As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_NutritionExercise As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_EstablishRoutineBedMealsToiletingEtc As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_UseSportProtection As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Bullying As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_OralHealth As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Others As New StringField
            <DataMember()>
            Public Property AppropriateEducation_SportInjuryPrevention As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_DrowningSunSafety As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_SafeAtHome As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_CorrectUseSeatbelt As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_SexualEducationSTD As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_DepresionAnxiety As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_RiskOfTattoosPiercing As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Autocontrol As New BooleanField
            <DataMember()>
            Public Property AppropriateEducation_Others_Checkbox As New BooleanField
        End Class

#End Region

#Region " Medication Review: Patient / Caregiver"

        <Serializable(), DataContract()>
        Public Class MedicalReviewSection
            Inherits AHASectionErrorSummary

            Private _Question1 As New BooleanField
            Private _Question2 As New BooleanField
            Private _Question3 As New BooleanField

            <DataMember()>
            Public Property Question1() As BooleanField
                Get
                    Return _Question1
                End Get
                Set(ByVal value As BooleanField)
                    _Question1 = value
                End Set
            End Property

            <DataMember()>
            Public Property Question2() As BooleanField
                Get
                    Return _Question2
                End Get
                Set(ByVal value As BooleanField)
                    _Question2 = value
                End Set
            End Property

            <DataMember()>
            Public Property Question3() As BooleanField
                Get
                    Return _Question3
                End Get
                Set(ByVal value As BooleanField)
                    _Question3 = value
                End Set
            End Property

        End Class

#End Region

#Region " Review of System (ROS)"

        <Serializable(), DataContract()>
        Public Class ReviewOfSystemSection
            Inherits AHASectionErrorSummary

            Private _Constitutional As New StringField
            Private _HEENTOral As New StringField
            Private _AllergicImmunologic As New StringField
            Private _HematologicLymphatic As New StringField
            Private _Cardiovascular As New StringField
            Private _Gastrointestinal As New StringField
            Private _Genitourinary As New StringField
            Private _Respiratory As New StringField
            Private _Musculoskeletal As New StringField
            Private _Neurological As New StringField
            Private _Endocrine As New StringField
            Private _Integumentary As New StringField
            Private _Psychiatric As New StringField
            Private _DescribePositiveROS As New StringField
            Private _RosItself As New BooleanField
            Private _HearingDifficulty As New IntegerField


            Public Sub New()
                Me.Constitutional() = New StringField()
                Me.HEENTOral() = New StringField()
                Me.AllergicImmunologic() = New StringField()
                Me.HematologicLymphatic() = New StringField()
                Me.Cardiovascular() = New StringField()
                Me.Gastrointestinal() = New StringField()
                Me.Genitourinary() = New StringField()
                Me.Respiratory() = New StringField()
                Me.Musculoskeletal() = New StringField()
                Me.Neurological() = New StringField()
                Me.Endocrine() = New StringField()
                Me.Integumentary() = New StringField()
                Me.Psychiatric() = New StringField()
                Me.UrinaryIncontinenceLeaking() = New BooleanField()
                Me.UrinaryIncontinence_BladderExercises() = New BooleanField()
                Me.UrinaryIncontinence_TreatmentWithMedicine() = New BooleanField()
                Me.UrinaryIncontinence_SurgicalIntervention() = New BooleanField()
                Me.UrinaryIncontinence_Other() = New StringField()
                Me.UrinaryIncontinence_CheckBoxOther() = New BooleanField()
                Me.DescribePositiveROS() = New StringField()
                Me.RosItself() = New BooleanField()

            End Sub



            <DataMember()>
            Public Property Constitutional() As StringField
                Get
                    Return _Constitutional
                End Get
                Set(ByVal value As StringField)
                    _Constitutional = value
                End Set
            End Property

            <DataMember()>
            Public Property HEENTOral() As StringField
                Get
                    Return _HEENTOral
                End Get
                Set(ByVal value As StringField)
                    _HEENTOral = value
                End Set
            End Property

            <DataMember()>
            Public Property AllergicImmunologic() As StringField
                Get
                    Return _AllergicImmunologic
                End Get
                Set(ByVal value As StringField)
                    _AllergicImmunologic = value
                End Set
            End Property

            <DataMember()>
            Public Property HematologicLymphatic() As StringField
                Get
                    Return _HematologicLymphatic
                End Get
                Set(ByVal value As StringField)
                    _HematologicLymphatic = value
                End Set
            End Property

            <DataMember()>
            Public Property Cardiovascular() As StringField
                Get
                    Return _Cardiovascular
                End Get
                Set(ByVal value As StringField)
                    _Cardiovascular = value
                End Set
            End Property

            <DataMember()>
            Public Property Gastrointestinal() As StringField
                Get
                    Return _Gastrointestinal
                End Get
                Set(ByVal value As StringField)
                    _Gastrointestinal = value
                End Set
            End Property

            <DataMember()>
            Public Property Genitourinary() As StringField
                Get
                    Return _Genitourinary
                End Get
                Set(ByVal value As StringField)
                    _Genitourinary = value
                End Set
            End Property

            <DataMember()>
            Public Property Respiratory() As StringField
                Get
                    Return _Respiratory
                End Get
                Set(ByVal value As StringField)
                    _Respiratory = value
                End Set
            End Property

            <DataMember()>
            Public Property Musculoskeletal() As StringField
                Get
                    Return _Musculoskeletal
                End Get
                Set(ByVal value As StringField)
                    _Musculoskeletal = value
                End Set
            End Property

            <DataMember()>
            Public Property Neurological() As StringField
                Get
                    Return _Neurological
                End Get
                Set(ByVal value As StringField)
                    _Neurological = value
                End Set
            End Property

            <DataMember()>
            Public Property Endocrine() As StringField
                Get
                    Return _Endocrine
                End Get
                Set(ByVal value As StringField)
                    _Endocrine = value
                End Set
            End Property

            <DataMember()>
            Public Property Integumentary() As StringField
                Get
                    Return _Integumentary
                End Get
                Set(ByVal value As StringField)
                    _Integumentary = value
                End Set
            End Property

            <DataMember()>
            Public Property Psychiatric() As StringField
                Get
                    Return _Psychiatric
                End Get
                Set(ByVal value As StringField)
                    _Psychiatric = value
                End Set
            End Property


            Private _UrinaryIncontinenceLeaking As New BooleanField
            <DataMember()>
            Public Property UrinaryIncontinenceLeaking() As BooleanField
                Get
                    Return _UrinaryIncontinenceLeaking
                End Get
                Set(ByVal value As BooleanField)
                    _UrinaryIncontinenceLeaking = value
                End Set
            End Property
            ' Actualizacion 2020 
            Private _UrinaryIncontinence_BladderExercises As New BooleanField
            <DataMember()>
            Public Property UrinaryIncontinence_BladderExercises() As BooleanField
                Get
                    Return _UrinaryIncontinence_BladderExercises
                End Get
                Set(ByVal value As BooleanField)
                    _UrinaryIncontinence_BladderExercises = value
                End Set
            End Property

            ' Actualizacion 2020 

            Private _UrinaryIncontinence_TreatmentWithMedicine As New BooleanField
            <DataMember()>
            Public Property UrinaryIncontinence_TreatmentWithMedicine() As BooleanField
                Get
                    Return _UrinaryIncontinence_TreatmentWithMedicine
                End Get
                Set(ByVal value As BooleanField)
                    _UrinaryIncontinence_TreatmentWithMedicine = value
                End Set
            End Property

            ' Actualizacion 2020 
            Private _UrinaryIncontinence_SurgicalIntervention As New BooleanField
            <DataMember()>
            Public Property UrinaryIncontinence_SurgicalIntervention() As BooleanField
                Get
                    Return _UrinaryIncontinence_SurgicalIntervention
                End Get
                Set(ByVal value As BooleanField)
                    _UrinaryIncontinence_SurgicalIntervention = value
                End Set
            End Property

            ' Actualizacion 2020 
            Private _UrinaryIncontinence_Other As New StringField
            <DataMember()>
            Public Property UrinaryIncontinence_Other() As StringField
                Get
                    Return _UrinaryIncontinence_Other
                End Get
                Set(ByVal value As StringField)
                    _UrinaryIncontinence_Other = value
                End Set
            End Property

            ' Actualizacion 2020 
            Private _UrinaryIncontinence_CheckBoxOther As New BooleanField
            <DataMember()>
            Public Property UrinaryIncontinence_CheckBoxOther() As BooleanField
                Get
                    Return _UrinaryIncontinence_CheckBoxOther
                End Get
                Set(ByVal value As BooleanField)
                    _UrinaryIncontinence_CheckBoxOther = value
                End Set
            End Property

            <DataMember()>
            Public Property DescribePositiveROS() As StringField
                Get
                    Return _DescribePositiveROS
                End Get
                Set(ByVal value As StringField)
                    _DescribePositiveROS = value
                End Set
            End Property

            <DataMember()>
            Public Property RosItself() As BooleanField
                Get
                    Return _RosItself
                End Get
                Set(value As BooleanField)
                    _RosItself = value
                End Set
            End Property

            'Actualizacion 2026'
            <DataMember()>
            Public Property HearingDifficulty() As IntegerField
                Get
                    Return _HearingDifficulty
                End Get
                Set(value As IntegerField)
                    _HearingDifficulty = value
                End Set
            End Property

        End Class

#End Region




#Region " Physical Examination"

        <Serializable(), DataContract()>
        Public Class PhysicalExaminationSection
            Inherits AHASectionErrorSummary

            Private _Temperature As New DecimalField

            Public Sub New()
                Me.Temperature() = New DecimalField()
                Me.TemperatureType() = New StringField()
                Me.Pulse() = New IntegerField()
                Me.Breathing() = New IntegerField()
                Me.BloodPresure1() = New IntegerField()
                Me.BloodPresure2() = New IntegerField()
                Me.Height() = New DecimalField()
                Me.HeightType() = New StringField()
                Me.Weight() = New DecimalField()
                Me.WeightType() = New StringField()
                Me.BMI() = New DecimalField()
                Me.HEENOralOptions() = New HEENOralOptionsSection()


                '2025
                Me.ConstitutionalOptions() = New ConstitutionalOptionsSection()
                Me.ConstitutionalNotes() = New StringField()
                Me.IntegumentaryOptions() = New IntegumentaryOptionsSection()
                Me.IntegumentaryNotes() = New StringField()
                Me.RespiratoryOptions() = New RespiratoryOptionsSection()
                Me.RespiratoryNotes() = New StringField()
                Me.GastrointestinalOptions = New GastrointestinalOptionsSection()
                Me.GastrointestinalNotes = New StringField()
                Me.GenitourinaryOptions = New GenitourinaryOptionsSection()
                Me.GenitourinaryNotes = New StringField


                Me.HEENOralNotes() = New StringField()
                Me.NeckOptions() = New NeckOptionSection()
                Me.NeckNotes() = New StringField()
                Me.ChestOption() = New ChestOptionSection()
                Me.ChestNotes() = New StringField()
                Me.CardiovascularOption() = New CardiovascularOptionSection()
                Me.CardiovascularNotes() = New StringField()
                Me.AbdomenOptions() = New AbdomenOptionSection()
                Me.AbdomenNotes() = New StringField()
                Me.GenitaliaGroinButtocksNotesOptions() = New GenitaliaGroinButtocksOptionSection()
                Me.GenitaliaGroinButtocksNotes() = New StringField()
                Me.MusculoskeletalNotes() = New StringField()
                Me.MusculoskeletalOptions() = New MusculoskeletalOptionSection()
                Me.SkinOptions() = New SkinOptionSection()
                Me.SkinNotes() = New StringField()
                Me.PsychiatricNeurologicOptions() = New PsychiatricNeurologicOptionSection()
                Me.PsychiatricNeurologicNotes() = New StringField()
                Me.HematologicLymphaticImmunologicOptions() = New HematologicLymphaticImmunologicOptionsSection()
                Me.HematologicLymphaticImmunologicNotes() = New StringField()
                Me.AmputationLegRT_BKA() = New BooleanField()
                Me.AmputationLegRT_AKA() = New BooleanField()
                Me.AmputationLegRT_Toe() = New BooleanField()
                Me.AmputationLegLT_BKA() = New BooleanField()
                Me.AmputationLegLT_AKA() = New BooleanField()
                Me.AmputationLegLT_Toe() = New BooleanField()
                Me.HeadCircumference = New DecimalField()
                Me.PercentilWT = New DecimalField()
                Me.PercentilHT = New DecimalField()
                Me.PercentilHead = New DecimalField()
            End Sub

            <DataMember()>
            Public Property Temperature() As DecimalField
                Get
                    Return _Temperature
                End Get
                Set(ByVal value As DecimalField)
                    _Temperature = value
                End Set
            End Property

            Private _TemperatureType As New StringField
            <DataMember()>
            Public Property TemperatureType() As StringField
                Get
                    Return _TemperatureType
                End Get
                Set(ByVal value As StringField)
                    _TemperatureType = value
                End Set
            End Property

            Private _Pulse As New IntegerField
            <DataMember()>
            Public Property Pulse() As IntegerField
                Get
                    Return _Pulse
                End Get
                Set(ByVal value As IntegerField)
                    _Pulse = value
                End Set
            End Property

            Private _Breathing As New IntegerField
            <DataMember()>
            Public Property Breathing() As IntegerField
                Get
                    Return _Breathing
                End Get
                Set(ByVal value As IntegerField)
                    _Breathing = value
                End Set
            End Property

            Private _BloodPresure1 As New IntegerField
            <DataMember()>
            Public Property BloodPresure1() As IntegerField
                Get
                    Return _BloodPresure1
                End Get
                Set(ByVal value As IntegerField)
                    _BloodPresure1 = value
                End Set
            End Property

            Private _BloodPresure2 As New IntegerField
            <DataMember()>
            Public Property BloodPresure2() As IntegerField
                Get
                    Return _BloodPresure2
                End Get
                Set(ByVal value As IntegerField)
                    _BloodPresure2 = value
                End Set
            End Property

            Private _Height As New DecimalField
            <DataMember()>
            Public Property Height() As DecimalField
                Get
                    Return _Height
                End Get
                Set(ByVal value As DecimalField)
                    _Height = value
                End Set
            End Property

            Private _HeightType As New StringField
            <DataMember()>
            Public Property HeightType() As StringField
                Get
                    Return _HeightType
                End Get
                Set(ByVal value As StringField)
                    _HeightType = value
                End Set
            End Property

            Private _Weight As New DecimalField
            <DataMember()>
            Public Property Weight() As DecimalField
                Get
                    Return _Weight
                End Get
                Set(ByVal value As DecimalField)
                    _Weight = value
                End Set
            End Property

            Private _WeightType As New StringField
            <DataMember()>
            Public Property WeightType() As StringField
                Get
                    Return _WeightType
                End Get
                Set(ByVal value As StringField)
                    _WeightType = value
                End Set
            End Property

            Private _BMI As New DecimalField
            <DataMember()>
            Public Property BMI() As DecimalField
                Get
                    Return _BMI
                End Get
                Set(ByVal value As DecimalField)
                    _BMI = value
                End Set
            End Property

            Private _HEENOralOptions As New HEENOralOptionsSection
            <DataMember()>
            Public Property HEENOralOptions() As HEENOralOptionsSection
                Get
                    Return _HEENOralOptions
                End Get
                Set(ByVal value As HEENOralOptionsSection)
                    _HEENOralOptions = value
                End Set
            End Property

            Private _HEENOralNotes As New StringField
            <DataMember()>
            Public Property HEENOralNotes() As StringField
                Get
                    Return _HEENOralNotes
                End Get
                Set(ByVal value As StringField)
                    _HEENOralNotes = value
                End Set
            End Property


            '2025
            Private _ConstitutionalOptions As New ConstitutionalOptionsSection
            <DataMember()>
            Public Property ConstitutionalOptions() As ConstitutionalOptionsSection
                Get
                    Return _ConstitutionalOptions
                End Get
                Set(ByVal value As ConstitutionalOptionsSection)
                    _ConstitutionalOptions = value
                End Set
            End Property

            Private _ConstitutionalNotes As New StringField
            <DataMember()>
            Public Property ConstitutionalNotes() As StringField
                Get
                    Return _ConstitutionalNotes
                End Get
                Set(ByVal value As StringField)
                    _ConstitutionalNotes = value
                End Set
            End Property

            Private _IntegumentaryOptions As New IntegumentaryOptionsSection
            <DataMember()>
            Public Property IntegumentaryOptions() As IntegumentaryOptionsSection
                Get
                    Return _IntegumentaryOptions
                End Get
                Set(ByVal value As IntegumentaryOptionsSection)
                    _IntegumentaryOptions = value
                End Set
            End Property

            Private _IntegumentaryNotes As New StringField
            <DataMember()>
            Public Property IntegumentaryNotes() As StringField
                Get
                    Return _IntegumentaryNotes
                End Get
                Set(ByVal value As StringField)
                    _IntegumentaryNotes = value
                End Set
            End Property

            Private _RespiratoryOptions As New RespiratoryOptionsSection
            <DataMember()>
            Public Property RespiratoryOptions() As RespiratoryOptionsSection
                Get
                    Return _RespiratoryOptions
                End Get
                Set(ByVal value As RespiratoryOptionsSection)
                    _RespiratoryOptions = value
                End Set
            End Property

            Private _RespiratoryNotes As New StringField
            <DataMember()>
            Public Property RespiratoryNotes() As StringField
                Get
                    Return _RespiratoryNotes
                End Get
                Set(ByVal value As StringField)
                    _RespiratoryNotes = value
                End Set
            End Property

            Private _GastrointestinalOptions As New GastrointestinalOptionsSection
            <DataMember()>
            Public Property GastrointestinalOptions() As GastrointestinalOptionsSection
                Get
                    Return _GastrointestinalOptions
                End Get
                Set(ByVal value As GastrointestinalOptionsSection)
                    _GastrointestinalOptions = value
                End Set
            End Property

            Private _GastrointestinalNotes As New StringField
            <DataMember()>
            Public Property GastrointestinalNotes() As StringField
                Get
                    Return _GastrointestinalNotes
                End Get
                Set(ByVal value As StringField)
                    _GastrointestinalNotes = value
                End Set
            End Property

            Private _GenitourinaryOptions As New GenitourinaryOptionsSection
            <DataMember()>
            Public Property GenitourinaryOptions() As GenitourinaryOptionsSection
                Get
                    Return _GenitourinaryOptions
                End Get
                Set(ByVal value As GenitourinaryOptionsSection)
                    _GenitourinaryOptions = value
                End Set
            End Property

            Private _GenitourinaryNotes As New StringField
            <DataMember()>
            Public Property GenitourinaryNotes() As StringField
                Get
                    Return _GenitourinaryNotes
                End Get
                Set(ByVal value As StringField)
                    _GenitourinaryNotes = value
                End Set
            End Property


            Private _NeckOptions As New NeckOptionSection
            <DataMember()>
            Public Property NeckOptions() As NeckOptionSection
                Get
                    Return _NeckOptions
                End Get
                Set(ByVal value As NeckOptionSection)
                    _NeckOptions = value
                End Set
            End Property

            Private _NeckNotes As New StringField
            <DataMember()>
            Public Property NeckNotes() As StringField
                Get
                    Return _NeckNotes
                End Get
                Set(ByVal value As StringField)
                    _NeckNotes = value
                End Set
            End Property

            Private _ChestOption As New ChestOptionSection
            <DataMember()>
            Public Property ChestOption() As ChestOptionSection
                Get
                    Return _ChestOption
                End Get
                Set(ByVal value As ChestOptionSection)
                    _ChestOption = value
                End Set
            End Property

            Private _ChestNotes As New StringField
            <DataMember()>
            Public Property ChestNotes() As StringField
                Get
                    Return _ChestNotes
                End Get
                Set(ByVal value As StringField)
                    _ChestNotes = value
                End Set
            End Property

            Private _CardiovascularOptions As New CardiovascularOptionSection
            <DataMember()>
            Public Property CardiovascularOption() As CardiovascularOptionSection
                Get
                    Return _CardiovascularOptions
                End Get
                Set(ByVal value As CardiovascularOptionSection)
                    _CardiovascularOptions = value
                End Set
            End Property

            Private _CardiovascularNotes As New StringField
            <DataMember()>
            Public Property CardiovascularNotes() As StringField
                Get
                    Return _CardiovascularNotes
                End Get
                Set(ByVal value As StringField)
                    _CardiovascularNotes = value
                End Set
            End Property

            Private _AbdomenOptions As New AbdomenOptionSection
            <DataMember()>
            Public Property AbdomenOptions() As AbdomenOptionSection
                Get
                    Return _AbdomenOptions
                End Get
                Set(ByVal value As AbdomenOptionSection)
                    _AbdomenOptions = value
                End Set
            End Property

            Private _AbdomenNotes As New StringField
            <DataMember()>
            Public Property AbdomenNotes() As StringField
                Get
                    Return _AbdomenNotes
                End Get
                Set(ByVal value As StringField)
                    _AbdomenNotes = value
                End Set
            End Property

            Private _GenitaliaGroinButtocksNotesOptions As New GenitaliaGroinButtocksOptionSection
            <DataMember()>
            Public Property GenitaliaGroinButtocksNotesOptions() As GenitaliaGroinButtocksOptionSection
                Get
                    Return _GenitaliaGroinButtocksNotesOptions
                End Get
                Set(ByVal value As GenitaliaGroinButtocksOptionSection)
                    _GenitaliaGroinButtocksNotesOptions = value
                End Set
            End Property

            Private _GenitaliaGroinButtocksNotes As New StringField
            <DataMember()>
            Public Property GenitaliaGroinButtocksNotes() As StringField
                Get
                    Return _GenitaliaGroinButtocksNotes
                End Get
                Set(ByVal value As StringField)
                    _GenitaliaGroinButtocksNotes = value
                End Set
            End Property

            Private _MusculoskeletalNotes As New StringField
            <DataMember()>
            Public Property MusculoskeletalNotes() As StringField
                Get
                    Return _MusculoskeletalNotes
                End Get
                Set(ByVal value As StringField)
                    _MusculoskeletalNotes = value
                End Set
            End Property

            Private _MusculoskeletalOptions As New MusculoskeletalOptionSection
            <DataMember()>
            Public Property MusculoskeletalOptions() As MusculoskeletalOptionSection
                Get
                    Return _MusculoskeletalOptions
                End Get
                Set(ByVal value As MusculoskeletalOptionSection)
                    _MusculoskeletalOptions = value
                End Set
            End Property

            Private _SkinOptions As New SkinOptionSection
            <DataMember()>
            Public Property SkinOptions() As SkinOptionSection
                Get
                    Return _SkinOptions
                End Get
                Set(ByVal value As SkinOptionSection)
                    _SkinOptions = value
                End Set
            End Property

            Private _SkinNotes As New StringField
            <DataMember()>
            Public Property SkinNotes() As StringField
                Get
                    Return _SkinNotes
                End Get
                Set(ByVal value As StringField)
                    _SkinNotes = value
                End Set
            End Property

            Private _PsychiatricNeurologicOptions As New PsychiatricNeurologicOptionSection
            <DataMember()>
            Public Property PsychiatricNeurologicOptions() As PsychiatricNeurologicOptionSection
                Get
                    Return _PsychiatricNeurologicOptions
                End Get
                Set(ByVal value As PsychiatricNeurologicOptionSection)
                    _PsychiatricNeurologicOptions = value
                End Set
            End Property

            Private _PsychiatricNeurologicNotes As New StringField
            <DataMember()>
            Public Property PsychiatricNeurologicNotes() As StringField
                Get
                    Return _PsychiatricNeurologicNotes
                End Get
                Set(ByVal value As StringField)
                    _PsychiatricNeurologicNotes = value
                End Set
            End Property

            Private _HematologicLymphaticImmunologicOptions As New HematologicLymphaticImmunologicOptionsSection
            <DataMember()>
            Public Property HematologicLymphaticImmunologicOptions() As HematologicLymphaticImmunologicOptionsSection
                Get
                    Return _HematologicLymphaticImmunologicOptions
                End Get
                Set(ByVal value As HematologicLymphaticImmunologicOptionsSection)
                    _HematologicLymphaticImmunologicOptions = value
                End Set
            End Property

            Private _HematologicLymphaticImmunologicNotes As New StringField
            <DataMember()>
            Public Property HematologicLymphaticImmunologicNotes() As StringField
                Get
                    Return _HematologicLymphaticImmunologicNotes
                End Get
                Set(ByVal value As StringField)
                    _HematologicLymphaticImmunologicNotes = value
                End Set
            End Property

            Private _AmputationLegRT_BKA As New BooleanField
            <DataMember()>
            Public Property AmputationLegRT_BKA() As BooleanField
                Get
                    Return _AmputationLegRT_BKA
                End Get
                Set(ByVal value As BooleanField)
                    _AmputationLegRT_BKA = value
                End Set
            End Property

            Private _AmputationLegRT_AKA As New BooleanField
            <DataMember()>
            Public Property AmputationLegRT_AKA() As BooleanField
                Get
                    Return _AmputationLegRT_AKA
                End Get
                Set(ByVal value As BooleanField)
                    _AmputationLegRT_AKA = value
                End Set
            End Property

            Private _AmputationLegRT_Toe As New BooleanField
            <DataMember()>
            Public Property AmputationLegRT_Toe() As BooleanField
                Get
                    Return _AmputationLegRT_Toe
                End Get
                Set(ByVal value As BooleanField)
                    _AmputationLegRT_Toe = value
                End Set
            End Property

            Private _AmputationLegLT_BKA As New BooleanField
            <DataMember()>
            Public Property AmputationLegLT_BKA() As BooleanField
                Get
                    Return _AmputationLegLT_BKA
                End Get
                Set(ByVal value As BooleanField)
                    _AmputationLegLT_BKA = value
                End Set
            End Property

            Private _AmputationLegLT_AKA As New BooleanField
            <DataMember()>
            Public Property AmputationLegLT_AKA() As BooleanField
                Get
                    Return _AmputationLegLT_AKA
                End Get
                Set(ByVal value As BooleanField)
                    _AmputationLegLT_AKA = value
                End Set
            End Property

            Private _AmputationLegLT_Toe As New BooleanField
            <DataMember()>
            Public Property AmputationLegLT_Toe() As BooleanField
                Get
                    Return _AmputationLegLT_Toe
                End Get
                Set(ByVal value As BooleanField)
                    _AmputationLegLT_Toe = value
                End Set
            End Property

            <DataMember()>
            Public Property HeadCircumference As New DecimalField
            <DataMember()>
            Public Property PercentilWT As New DecimalField
            <DataMember()>
            Public Property PercentilHT As New DecimalField
            <DataMember()>
            Public Property PercentilHead As New DecimalField

        End Class

        <Serializable(), DataContract()>
        Public Class HEENOralOptionsSection

            Private _PEERL As New BooleanField
            <DataMember()>
            Public Property PEERL() As BooleanField
                Get
                    Return _PEERL
                End Get
                Set(ByVal value As BooleanField)
                    _PEERL = value
                End Set
            End Property

            Private _NoTeeth As New BooleanField
            <DataMember()>
            Public Property NoTeeth() As BooleanField
                Get
                    Return _NoTeeth
                End Get
                Set(ByVal value As BooleanField)
                    _NoTeeth = value
                End Set
            End Property

            Private _DryMouth As New BooleanField
            <DataMember()>
            Public Property DryMouth() As BooleanField
                Get
                    Return _DryMouth
                End Get
                Set(ByVal value As BooleanField)
                    _DryMouth = value
                End Set
            End Property

            Private _DryNose As New BooleanField
            <DataMember()>
            Public Property DryNose() As BooleanField
                Get
                    Return _DryNose
                End Get
                Set(ByVal value As BooleanField)
                    _DryNose = value
                End Set
            End Property

            Private _BleedingGums As New BooleanField
            <DataMember()>
            Public Property BleedingGums() As BooleanField
                Get
                    Return _BleedingGums
                End Get
                Set(ByVal value As BooleanField)
                    _BleedingGums = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            Private _Strabismus As New BooleanField
            <DataMember()>
            Public Property Strabismus() As BooleanField
                Get
                    Return _Strabismus
                End Get
                Set(ByVal value As BooleanField)
                    _Strabismus = value
                End Set
            End Property


            Private _Ptosis As New BooleanField
            <DataMember()>
            Public Property Ptosis() As BooleanField
                Get
                    Return _Ptosis
                End Get
                Set(ByVal value As BooleanField)
                    _Ptosis = value
                End Set
            End Property

            Private _RedReflex As New BooleanField
            <DataMember()>
            Public Property RedReflex() As BooleanField
                Get
                    Return _RedReflex
                End Get
                Set(ByVal value As BooleanField)
                    _RedReflex = value
                End Set
            End Property

            Private _AbnormalPupillaryReflex As New BooleanField
            <DataMember()>
            Public Property AbnormalPupillaryReflex() As BooleanField
                Get
                    Return _AbnormalPupillaryReflex
                End Get
                Set(ByVal value As BooleanField)
                    _AbnormalPupillaryReflex = value
                End Set
            End Property

            Private _BlockedNasolacrimalDucts As New BooleanField
            <DataMember()>
            Public Property BlockedNasolacrimalDucts() As BooleanField
                Get
                    Return _BlockedNasolacrimalDucts
                End Get
                Set(ByVal value As BooleanField)
                    _BlockedNasolacrimalDucts = value
                End Set
            End Property

            Private _NasalDischarge As New BooleanField
            <DataMember()>
            Public Property NasalDischarge() As BooleanField
                Get
                    Return _NasalDischarge
                End Get
                Set(ByVal value As BooleanField)
                    _NasalDischarge = value
                End Set
            End Property

            Private _ExudatingTonsils As New BooleanField
            <DataMember()>
            Public Property ExudatingTonsils() As BooleanField
                Get
                    Return _ExudatingTonsils
                End Get
                Set(ByVal value As BooleanField)
                    _ExudatingTonsils = value
                End Set
            End Property


            '2025
            Private _Normocephalic As New BooleanField
            <DataMember()>
            Public Property Normocephalic() As BooleanField
                Get
                    Return _Normocephalic
                End Get
                Set(ByVal value As BooleanField)
                    _Normocephalic = value
                End Set
            End Property

            Private _ScalpLessionsMasses As New BooleanField
            <DataMember()>
            Public Property ScalpLessionsMasses() As BooleanField
                Get
                    Return _ScalpLessionsMasses
                End Get
                Set(ByVal value As BooleanField)
                    _ScalpLessionsMasses = value
                End Set
            End Property

            Private _NeckSupple As New BooleanField
            <DataMember()>
            Public Property NeckSupple() As BooleanField
                Get
                    Return _NeckSupple
                End Get
                Set(ByVal value As BooleanField)
                    _NeckSupple = value
                End Set
            End Property

            Private _Adenopathies As New BooleanField
            <DataMember()>
            Public Property Adenopathies() As BooleanField
                Get
                    Return _Adenopathies
                End Get
                Set(ByVal value As BooleanField)
                    _Adenopathies = value
                End Set
            End Property

            Private _ClearOropharynxn As New BooleanField
            <DataMember()>
            Public Property ClearOropharynxn() As BooleanField
                Get
                    Return _ClearOropharynxn
                End Get
                Set(ByVal value As BooleanField)
                    _ClearOropharynxn = value
                End Set
            End Property

            Private _LessionExudate As New BooleanField
            <DataMember()>
            Public Property LessionExudate() As BooleanField
                Get
                    Return _LessionExudate
                End Get
                Set(ByVal value As BooleanField)
                    _LessionExudate = value
                End Set
            End Property

            Private _TympanicMembranesIntact As New BooleanField
            <DataMember()>
            Public Property TympanicMembranesIntact() As BooleanField
                Get
                    Return _TympanicMembranesIntact
                End Get
                Set(ByVal value As BooleanField)
                    _TympanicMembranesIntact = value
                End Set
            End Property

            Private _EqualAirConductionAndAcousticReflexes As New BooleanField
            <DataMember()>
            Public Property EqualAirConductionAndAcousticReflexes() As BooleanField
                Get
                    Return _EqualAirConductionAndAcousticReflexes
                End Get
                Set(ByVal value As BooleanField)
                    _EqualAirConductionAndAcousticReflexes = value
                End Set
            End Property

            Private _NoNystagmus As New BooleanField
            <DataMember()>
            Public Property NoNystagmus() As BooleanField
                Get
                    Return _NoNystagmus
                End Get
                Set(ByVal value As BooleanField)
                    _NoNystagmus = value
                End Set
            End Property

            Private _EOMI As New BooleanField
            <DataMember()>
            Public Property EOMI() As BooleanField
                Get
                    Return _EOMI
                End Get
                Set(ByVal value As BooleanField)
                    _EOMI = value
                End Set
            End Property

            Private _Other As New BooleanField
            <DataMember()>
            Public Property Other() As BooleanField
                Get
                    Return _Other
                End Get
                Set(ByVal value As BooleanField)
                    _Other = value
                End Set
            End Property

            Private _None As New BooleanField
            <DataMember()>
            Public Property None() As BooleanField
                Get
                    Return _None
                End Get
                Set(ByVal value As BooleanField)
                    _None = value
                End Set
            End Property

        End Class


        '2025
        <Serializable(), DataContract()>
        Public Class ConstitutionalOptionsSection

            Private _WellDeveloped As New BooleanField
            <DataMember()>
            Public Property WellDeveloped() As BooleanField
                Get
                    Return _WellDeveloped
                End Get
                Set(ByVal value As BooleanField)
                    _WellDeveloped = value
                End Set
            End Property

            Private _PoorDeveloped As New BooleanField
            <DataMember()>
            Public Property PoorDeveloped() As BooleanField
                Get
                    Return _PoorDeveloped
                End Get
                Set(ByVal value As BooleanField)
                    _PoorDeveloped = value
                End Set
            End Property

            Private _AdequateNourishment As New BooleanField
            <DataMember()>
            Public Property AdequateNourishment() As BooleanField
                Get
                    Return _AdequateNourishment
                End Get
                Set(ByVal value As BooleanField)
                    _AdequateNourishment = value
                End Set
            End Property

            Private _InadequateNourishment As New BooleanField
            <DataMember()>
            Public Property InadequateNourishment() As BooleanField
                Get
                    Return _InadequateNourishment
                End Get
                Set(ByVal value As BooleanField)
                    _InadequateNourishment = value
                End Set
            End Property

            Private _InAcuteDistress As New BooleanField
            <DataMember()>
            Public Property InAcuteDistress() As BooleanField
                Get
                    Return _InAcuteDistress
                End Get
                Set(ByVal value As BooleanField)
                    _InAcuteDistress = value
                End Set
            End Property

            Private _NoAcuteDistress As New BooleanField
            <DataMember()>
            Public Property NoAcuteDistress() As BooleanField
                Get
                    Return _NoAcuteDistress
                End Get
                Set(ByVal value As BooleanField)
                    _NoAcuteDistress = value
                End Set
            End Property

            Private _CAOX As New BooleanField
            <DataMember()>
            Public Property CAOX() As BooleanField
                Get
                    Return _CAOX
                End Get
                Set(ByVal value As BooleanField)
                    _CAOX = value
                End Set
            End Property

            Private _Others As New BooleanField
            <DataMember()>
            Public Property Others() As BooleanField
                Get
                    Return _Others
                End Get
                Set(ByVal value As BooleanField)
                    _Others = value
                End Set
            End Property

            Private _None As New BooleanField
            <DataMember()>
            Public Property None() As BooleanField
                Get
                    Return _None
                End Get
                Set(ByVal value As BooleanField)
                    _None = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class IntegumentaryOptionsSection

            Private _Warm As New BooleanField
            <DataMember()>
            Public Property Warm() As BooleanField
                Get
                    Return _Warm
                End Get
                Set(ByVal value As BooleanField)
                    _Warm = value
                End Set
            End Property

            Private _Cold As New BooleanField
            <DataMember()>
            Public Property Cold() As BooleanField
                Get
                    Return _Cold
                End Get
                Set(ByVal value As BooleanField)
                    _Cold = value
                End Set
            End Property

            Private _AdequatePerfusion As New BooleanField
            <DataMember()>
            Public Property AdequatePerfusion() As BooleanField
                Get
                    Return _AdequatePerfusion
                End Get
                Set(ByVal value As BooleanField)
                    _AdequatePerfusion = value
                End Set
            End Property

            Private _InadequatePerfusion As New BooleanField
            <DataMember()>
            Public Property InadequatePerfusion() As BooleanField
                Get
                    Return _InadequatePerfusion
                End Get
                Set(ByVal value As BooleanField)
                    _InadequatePerfusion = value
                End Set
            End Property

            Private _AdequateSkinTurgor As New BooleanField
            <DataMember()>
            Public Property AdequateSkinTurgor() As BooleanField
                Get
                    Return _AdequateSkinTurgor
                End Get
                Set(ByVal value As BooleanField)
                    _AdequateSkinTurgor = value
                End Set
            End Property

            Private _InadequateSkinTurgor As New BooleanField
            <DataMember()>
            Public Property InadequateSkinTurgor() As BooleanField
                Get
                    Return _InadequateSkinTurgor
                End Get
                Set(ByVal value As BooleanField)
                    _InadequateSkinTurgor = value
                End Set
            End Property

            Private _Acne As New BooleanField
            <DataMember()>
            Public Property Acne() As BooleanField
                Get
                    Return _Acne
                End Get
                Set(ByVal value As BooleanField)
                    _Acne = value
                End Set
            End Property

            Private _Rash As New BooleanField
            <DataMember()>
            Public Property Rash() As BooleanField
                Get
                    Return _Rash
                End Get
                Set(ByVal value As BooleanField)
                    _Rash = value
                End Set
            End Property

            Private _SkinSpots As New BooleanField
            <DataMember()>
            Public Property SkinSpots() As BooleanField
                Get
                    Return _SkinSpots
                End Get
                Set(ByVal value As BooleanField)
                    _SkinSpots = value
                End Set
            End Property

            Private _Others As New BooleanField
            <DataMember()>
            Public Property Others() As BooleanField
                Get
                    Return _Others
                End Get
                Set(ByVal value As BooleanField)
                    _Others = value
                End Set
            End Property

            Private _None As New BooleanField
            <DataMember()>
            Public Property None() As BooleanField
                Get
                    Return _None
                End Get
                Set(ByVal value As BooleanField)
                    _None = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class RespiratoryOptionsSection

            Private _ClearToAuscultations As New BooleanField
            <DataMember()>
            Public Property ClearToAuscultations() As BooleanField
                Get
                    Return _ClearToAuscultations
                End Get
                Set(ByVal value As BooleanField)
                    _ClearToAuscultations = value
                End Set
            End Property

            Private _Wheezes As New BooleanField
            <DataMember()>
            Public Property Wheezes() As BooleanField
                Get
                    Return _Wheezes
                End Get
                Set(ByVal value As BooleanField)
                    _Wheezes = value
                End Set
            End Property

            Private _RonchiOrRales As New BooleanField
            <DataMember()>
            Public Property RonchiOrRales() As BooleanField
                Get
                    Return _RonchiOrRales
                End Get
                Set(ByVal value As BooleanField)
                    _RonchiOrRales = value
                End Set
            End Property

            Private _AdequatePercussionSounds As New BooleanField
            <DataMember()>
            Public Property AdequatePercussionSounds() As BooleanField
                Get
                    Return _AdequatePercussionSounds
                End Get
                Set(ByVal value As BooleanField)
                    _AdequatePercussionSounds = value
                End Set
            End Property

            Private _InadequatePercussionSounds As New BooleanField
            <DataMember()>
            Public Property InadequatePercussionSounds() As BooleanField
                Get
                    Return _InadequatePercussionSounds
                End Get
                Set(ByVal value As BooleanField)
                    _InadequatePercussionSounds = value
                End Set
            End Property

            Private _PainUponPalpitation As New BooleanField
            <DataMember()>
            Public Property PainUponPalpitation() As BooleanField
                Get
                    Return _PainUponPalpitation
                End Get
                Set(ByVal value As BooleanField)
                    _PainUponPalpitation = value
                End Set
            End Property

            Private _Others As New BooleanField
            <DataMember()>
            Public Property Others() As BooleanField
                Get
                    Return _Others
                End Get
                Set(ByVal value As BooleanField)
                    _Others = value
                End Set
            End Property

            Private _None As New BooleanField
            <DataMember()>
            Public Property None() As BooleanField
                Get
                    Return _None
                End Get
                Set(ByVal value As BooleanField)
                    _None = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class GastrointestinalOptionsSection

            Private _GoodDentation As New BooleanField
            <DataMember()>
            Public Property GoodDentation() As BooleanField
                Get
                    Return _GoodDentation
                End Get
                Set(ByVal value As BooleanField)
                    _GoodDentation = value
                End Set
            End Property

            Private _PoorDentation As New BooleanField
            <DataMember()>
            Public Property PoorDentation() As BooleanField
                Get
                    Return _PoorDentation
                End Get
                Set(ByVal value As BooleanField)
                    _PoorDentation = value
                End Set
            End Property

            Private _HardToPalpation As New BooleanField
            <DataMember()>
            Public Property HardToPalpation() As BooleanField
                Get
                    Return _HardToPalpation
                End Get
                Set(ByVal value As BooleanField)
                    _HardToPalpation = value
                End Set
            End Property

            Private _SoftToPalpation As New BooleanField
            <DataMember()>
            Public Property SoftToPalpation() As BooleanField
                Get
                    Return _SoftToPalpation
                End Get
                Set(ByVal value As BooleanField)
                    _SoftToPalpation = value
                End Set
            End Property

            Private _Tenderness As New BooleanField
            <DataMember()>
            Public Property Tenderness() As BooleanField
                Get
                    Return _Tenderness
                End Get
                Set(ByVal value As BooleanField)
                    _Tenderness = value
                End Set
            End Property

            Private _Visceromegaly As New BooleanField
            <DataMember()>
            Public Property Visceromegaly() As BooleanField
                Get
                    Return _Visceromegaly
                End Get
                Set(ByVal value As BooleanField)
                    _Visceromegaly = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class GenitourinaryOptionsSection

            Private _DeferedGeneralAppereance As New BooleanField
            <DataMember()>
            Public Property DeferedGeneralAppereance() As BooleanField
                Get
                    Return _DeferedGeneralAppereance
                End Get
                Set(ByVal value As BooleanField)
                    _DeferedGeneralAppereance = value
                End Set
            End Property

            Private _WhithinNormalLimits As New BooleanField
            <DataMember()>
            Public Property WhithinNormalLimits() As BooleanField
                Get
                    Return _WhithinNormalLimits
                End Get
                Set(ByVal value As BooleanField)
                    _WhithinNormalLimits = value
                End Set
            End Property

        End Class



        <Serializable(), DataContract()>
        Public Class NeckOptionSection

            Private _Masses As New BooleanField
            <DataMember()>
            Public Property Masses() As BooleanField
                Get
                    Return _Masses
                End Get
                Set(ByVal value As BooleanField)
                    _Masses = value
                End Set
            End Property

            Private _OverallAppearance As New BooleanField
            <DataMember()>
            Public Property OverallAppearance() As BooleanField
                Get
                    Return _OverallAppearance
                End Get
                Set(ByVal value As BooleanField)
                    _OverallAppearance = value
                End Set
            End Property

            Private _Symmetry As New BooleanField
            <DataMember()>
            Public Property Symmetry() As BooleanField
                Get
                    Return _Symmetry
                End Get
                Set(ByVal value As BooleanField)
                    _Symmetry = value
                End Set
            End Property

            Private _NormalTrachealPosition As New BooleanField
            <DataMember()>
            Public Property NormalTrachealPosition() As BooleanField
                Get
                    Return _NormalTrachealPosition
                End Get
                Set(ByVal value As BooleanField)
                    _NormalTrachealPosition = value
                End Set
            End Property

            Private _Tracheostomy As New BooleanField
            <DataMember()>
            Public Property Tracheostomy() As BooleanField
                Get
                    Return _Tracheostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Tracheostomy = value
                End Set
            End Property

            Private _Crepitus As New BooleanField
            <DataMember()>
            Public Property Crepitus() As BooleanField
                Get
                    Return _Crepitus
                End Get
                Set(ByVal value As BooleanField)
                    _Crepitus = value
                End Set
            End Property


            Private _ThyroidEnlargement As New BooleanField
            <DataMember()>
            Public Property ThyroidEnlargement() As BooleanField
                Get
                    Return _ThyroidEnlargement
                End Get
                Set(ByVal value As BooleanField)
                    _ThyroidEnlargement = value
                End Set
            End Property

            Private _ThyroidTenderness As New BooleanField
            <DataMember()>
            Public Property ThyroidTenderness() As BooleanField
                Get
                    Return _ThyroidTenderness
                End Get
                Set(ByVal value As BooleanField)
                    _ThyroidTenderness = value
                End Set
            End Property

            Private _ThyroidMass As New BooleanField
            <DataMember()>
            Public Property ThyroidMass() As BooleanField
                Get
                    Return _ThyroidMass
                End Get
                Set(ByVal value As BooleanField)
                    _ThyroidMass = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            <DataMember()>
            Public Property NeckOption_Rigity As New BooleanField
            <DataMember()>
            Public Property NeckOption_MovementLimitation As New BooleanField
            <DataMember()>
            Public Property NeckOption_Crackle As New BooleanField

        End Class

        <Serializable(), DataContract()>
        Public Class ChestOptionSection

            Private _IntercostalRetractions As New BooleanField
            <DataMember()>
            Public Property IntercostalRetractions() As BooleanField
                Get
                    Return _IntercostalRetractions
                End Get
                Set(ByVal value As BooleanField)
                    _IntercostalRetractions = value
                End Set
            End Property

            Private _UseOfAccesoryMuscles As New BooleanField
            <DataMember()>
            Public Property UseOfAccesoryMuscles() As BooleanField
                Get
                    Return _UseOfAccesoryMuscles
                End Get
                Set(ByVal value As BooleanField)
                    _UseOfAccesoryMuscles = value
                End Set
            End Property

            Private _DiaphragmaticMovement As New BooleanField
            <DataMember()>
            Public Property DiaphragmaticMovement() As BooleanField
                Get
                    Return _DiaphragmaticMovement
                End Get
                Set(ByVal value As BooleanField)
                    _DiaphragmaticMovement = value
                End Set
            End Property

            Private _Dullness As New BooleanField
            <DataMember()>
            Public Property Dullness() As BooleanField
                Get
                    Return _Dullness
                End Get
                Set(ByVal value As BooleanField)
                    _Dullness = value
                End Set
            End Property

            Private _Flatness As New BooleanField
            <DataMember()>
            Public Property Flatness() As BooleanField
                Get
                    Return _Flatness
                End Get
                Set(ByVal value As BooleanField)
                    _Flatness = value
                End Set
            End Property

            Private _Hyperresonance As New BooleanField
            <DataMember()>
            Public Property Hyperresonance() As BooleanField
                Get
                    Return _Hyperresonance
                End Get
                Set(ByVal value As BooleanField)
                    _Hyperresonance = value
                End Set
            End Property

            Private _TactileFremitus As New BooleanField
            <DataMember()>
            Public Property TactileFremitus() As BooleanField
                Get
                    Return _TactileFremitus
                End Get
                Set(ByVal value As BooleanField)
                    _TactileFremitus = value
                End Set
            End Property

            Private _NormalBreathSounds As New BooleanField
            <DataMember()>
            Public Property NormalBreathSounds() As BooleanField
                Get
                    Return _NormalBreathSounds
                End Get
                Set(ByVal value As BooleanField)
                    _NormalBreathSounds = value
                End Set
            End Property

            Private _AdventitiousSounds As New BooleanField
            <DataMember()>
            Public Property AdventitiousSounds() As BooleanField
                Get
                    Return _AdventitiousSounds
                End Get
                Set(ByVal value As BooleanField)
                    _AdventitiousSounds = value
                End Set
            End Property


            Private _Rubs As New BooleanField
            <DataMember()>
            Public Property Rubs() As BooleanField
                Get
                    Return _Rubs
                End Get
                Set(ByVal value As BooleanField)
                    _Rubs = value
                End Set
            End Property

            Private _Crackels As New BooleanField
            <DataMember()>
            Public Property Crackels() As BooleanField
                Get
                    Return _Crackels
                End Get
                Set(ByVal value As BooleanField)
                    _Crackels = value
                End Set
            End Property

            Private _WheezingSymmetryBreasts As New BooleanField
            <DataMember()>
            Public Property WheezingSymmetryBreasts() As BooleanField
                Get
                    Return _WheezingSymmetryBreasts
                End Get
                Set(ByVal value As BooleanField)
                    _WheezingSymmetryBreasts = value
                End Set
            End Property

            Private _NippleDischargeBreastsMassesLumps As New BooleanField
            <DataMember()>
            Public Property NippleDischargeBreastsMassesLumps() As BooleanField
                Get
                    Return _NippleDischargeBreastsMassesLumps
                End Get
                Set(ByVal value As BooleanField)
                    _NippleDischargeBreastsMassesLumps = value
                End Set
            End Property

            Private _BreastsTenderness As New BooleanField
            <DataMember()>
            Public Property BreastsTenderness() As BooleanField
                Get
                    Return _BreastsTenderness
                End Get
                Set(ByVal value As BooleanField)
                    _BreastsTenderness = value
                End Set
            End Property

            Private _RTFootToeAmputation As New BooleanField
            <DataMember()>
            Public Property RTFootToeAmputation() As BooleanField
                Get
                    Return _RTFootToeAmputation
                End Get
                Set(ByVal value As BooleanField)
                    _RTFootToeAmputation = value
                End Set
            End Property

            Private _LTFootToeAmputation As New BooleanField
            <DataMember()>
            Public Property LTFootToeAmputation() As BooleanField
                Get
                    Return _LTFootToeAmputation
                End Get
                Set(ByVal value As BooleanField)
                    _LTFootToeAmputation = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property




            <DataMember()>
            Public Property NippleDischarge As New BooleanField
            <DataMember()>
            Public Property BreastsMasses As New BooleanField

        End Class

        <Serializable(), DataContract()>
        Public Class CardiovascularOptionSection

            Private _RTFootToeAmputation As New BooleanField
            <DataMember()>
            Public Property RTFootToeAmputation() As BooleanField
                Get
                    Return _RTFootToeAmputation
                End Get
                Set(ByVal value As BooleanField)
                    _RTFootToeAmputation = value
                End Set
            End Property

            Private _LTFootToeAmputation As New BooleanField
            <DataMember()>
            Public Property LTFootToeAmputation() As BooleanField
                Get
                    Return _LTFootToeAmputation
                End Get
                Set(ByVal value As BooleanField)
                    _LTFootToeAmputation = value
                End Set
            End Property

            Private _AbnormalHeartSound As New BooleanField
            <DataMember()>
            Public Property AbnormalHeartSound() As BooleanField
                Get
                    Return _AbnormalHeartSound
                End Get
                Set(ByVal value As BooleanField)
                    _AbnormalHeartSound = value
                End Set
            End Property

            Private _MurmursDecreasedPedalPulses As New BooleanField
            <DataMember()>
            Public Property MurmursDecreasedPedalPulses() As BooleanField
                Get
                    Return _MurmursDecreasedPedalPulses
                End Get
                Set(ByVal value As BooleanField)
                    _MurmursDecreasedPedalPulses = value
                End Set
            End Property

            Private _LegEdema As New BooleanField
            <DataMember()>
            Public Property LegEdema() As BooleanField
                Get
                    Return _LegEdema
                End Get
                Set(ByVal value As BooleanField)
                    _LegEdema = value
                End Set
            End Property


            Private _Varicosities As New BooleanField
            <DataMember()>
            Public Property Varicosities() As BooleanField
                Get
                    Return _Varicosities
                End Get
                Set(ByVal value As BooleanField)
                    _Varicosities = value
                End Set
            End Property

            Private _AbnormalTemperature As New BooleanField
            <DataMember()>
            Public Property AbnormalTemperature() As BooleanField
                Get
                    Return _AbnormalTemperature
                End Get
                Set(ByVal value As BooleanField)
                    _AbnormalTemperature = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            <DataMember()>
            Public Property DecreasedPedalPulses As New BooleanField


            '2025
            Private _RegularRateRhytm As New BooleanField
            <DataMember()>
            Public Property RegularRateRhytm() As BooleanField
                Get
                    Return _RegularRateRhytm
                End Get
                Set(ByVal value As BooleanField)
                    _RegularRateRhytm = value
                End Set
            End Property

            Private _IrregularRateRhytm As New BooleanField
            <DataMember()>
            Public Property IrregularRateRhytm() As BooleanField
                Get
                    Return _IrregularRateRhytm
                End Get
                Set(ByVal value As BooleanField)
                    _IrregularRateRhytm = value
                End Set
            End Property

            Private _Murmurs As New BooleanField
            <DataMember()>
            Public Property Murmurs() As BooleanField
                Get
                    Return _Murmurs
                End Get
                Set(ByVal value As BooleanField)
                    _Murmurs = value
                End Set
            End Property

            Private _Gallops As New BooleanField
            <DataMember()>
            Public Property Gallops() As BooleanField
                Get
                    Return _Gallops
                End Get
                Set(ByVal value As BooleanField)
                    _Gallops = value
                End Set
            End Property

            Private _Rubs As New BooleanField
            <DataMember()>
            Public Property Rubs() As BooleanField
                Get
                    Return _Rubs
                End Get
                Set(ByVal value As BooleanField)
                    _Rubs = value
                End Set
            End Property

            Private _PainUponPrecordialPalpation As New BooleanField
            <DataMember()>
            Public Property PainUponPrecordialPalpation() As BooleanField
                Get
                    Return _PainUponPrecordialPalpation
                End Get
                Set(ByVal value As BooleanField)
                    _PainUponPrecordialPalpation = value
                End Set
            End Property

            Private _Other As New BooleanField
            <DataMember()>
            Public Property Other() As BooleanField
                Get
                    Return _Other
                End Get
                Set(ByVal value As BooleanField)
                    _Other = value
                End Set
            End Property

            Private _None As New BooleanField
            <DataMember()>
            Public Property None() As BooleanField
                Get
                    Return _None
                End Get
                Set(ByVal value As BooleanField)
                    _None = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class AbdomenOptionSection

            Private _Masses As New BooleanField
            <DataMember()>
            Public Property Masses() As BooleanField
                Get
                    Return _Masses
                End Get
                Set(ByVal value As BooleanField)
                    _Masses = value
                End Set
            End Property

            Private _Tenderness As New BooleanField
            <DataMember()>
            Public Property Tenderness() As BooleanField
                Get
                    Return _Tenderness
                End Get
                Set(ByVal value As BooleanField)
                    _Tenderness = value
                End Set
            End Property

            Private _Hernia As New BooleanField
            <DataMember()>
            Public Property Hernia() As BooleanField
                Get
                    Return _Hernia
                End Get
                Set(ByVal value As BooleanField)
                    _Hernia = value
                End Set
            End Property

            Private _LiverEnlargement As New BooleanField
            <DataMember()>
            Public Property LiverEnlargement() As BooleanField
                Get
                    Return _LiverEnlargement
                End Get
                Set(ByVal value As BooleanField)
                    _LiverEnlargement = value
                End Set
            End Property

            Private _SpleenEnlargement As New BooleanField
            <DataMember()>
            Public Property SpleenEnlargement() As BooleanField
                Get
                    Return _SpleenEnlargement
                End Get
                Set(ByVal value As BooleanField)
                    _SpleenEnlargement = value
                End Set
            End Property

            Private _Colostomy As New BooleanField
            <DataMember()>
            Public Property Colostomy() As BooleanField
                Get
                    Return _Colostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Colostomy = value
                End Set
            End Property

            Private _Ileostomy As New BooleanField
            <DataMember()>
            Public Property Ileostomy() As BooleanField
                Get
                    Return _Ileostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Ileostomy = value
                End Set
            End Property

            Private _Gastrostomy As New BooleanField
            <DataMember()>
            Public Property Gastrostomy() As BooleanField
                Get
                    Return _Gastrostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Gastrostomy = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            Private _Cystostomy As New BooleanField
            <DataMember()>
            Public Property Cystostomy() As BooleanField
                Get
                    Return _Cystostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Cystostomy = value
                End Set
            End Property

            <DataMember()>
            Public Property AbdomenOptions_UmbilicalInfection As New BooleanField
            <DataMember()>
            Public Property AbdomenOptions_Distention As New BooleanField
            <DataMember()>
            Public Property AbdomenOptions_Constipation As New BooleanField
            <DataMember()>
            Public Property AbdomenOptions_Colics As New BooleanField
            <DataMember()>
            Public Property AbdomenOptions_Reflux As New BooleanField
            <DataMember()>
            Public Property AbdomenOptions_Rebound As New BooleanField
            <DataMember()>
            Public Property AbdomenOptions_Guarding As New BooleanField

        End Class

        <Serializable(), DataContract()>
        Public Class GenitaliaGroinButtocksOptionSection

            Private _DefferedGeneralAppearance As New BooleanField
            <DataMember()>
            Public Property DefferedGeneralAppearance() As BooleanField
                Get
                    Return _DefferedGeneralAppearance
                End Get
                Set(ByVal value As BooleanField)
                    _DefferedGeneralAppearance = value
                End Set
            End Property

            Private _HairDistribution As New BooleanField
            <DataMember()>
            Public Property HairDistribution() As BooleanField
                Get
                    Return _HairDistribution
                End Get
                Set(ByVal value As BooleanField)
                    _HairDistribution = value
                End Set
            End Property

            Private _Lesions As New BooleanField
            <DataMember()>
            Public Property Lesions() As BooleanField
                Get
                    Return _Lesions
                End Get
                Set(ByVal value As BooleanField)
                    _Lesions = value
                End Set
            End Property

            Private _Cyst As New BooleanField
            <DataMember()>
            Public Property Cyst() As BooleanField
                Get
                    Return _Cyst
                End Get
                Set(ByVal value As BooleanField)
                    _Cyst = value
                End Set
            End Property

            Private _Rashes As New BooleanField
            <DataMember()>
            Public Property Rashes() As BooleanField
                Get
                    Return _Rashes
                End Get
                Set(ByVal value As BooleanField)
                    _Rashes = value
                End Set
            End Property

            Private _Size As New BooleanField
            <DataMember()>
            Public Property Size() As BooleanField
                Get
                    Return _Size
                End Get
                Set(ByVal value As BooleanField)
                    _Size = value
                End Set
            End Property

            Private _Symmetry As New BooleanField
            <DataMember()>
            Public Property Symmetry() As BooleanField
                Get
                    Return _Symmetry
                End Get
                Set(ByVal value As BooleanField)
                    _Symmetry = value
                End Set
            End Property

            Private _Masses As New BooleanField
            <DataMember()>
            Public Property Masses() As BooleanField
                Get
                    Return _Masses
                End Get
                Set(ByVal value As BooleanField)
                    _Masses = value
                End Set
            End Property

            Private _Discharge As New BooleanField
            <DataMember()>
            Public Property Discharge() As BooleanField
                Get
                    Return _Discharge
                End Get
                Set(ByVal value As BooleanField)
                    _Discharge = value
                End Set
            End Property

            Private _Scarring As New BooleanField
            <DataMember()>
            Public Property Scarring() As BooleanField
                Get
                    Return _Scarring
                End Get
                Set(ByVal value As BooleanField)
                    _Scarring = value
                End Set
            End Property

            Private _Deformities As New BooleanField
            <DataMember()>
            Public Property Deformities() As BooleanField
                Get
                    Return _Deformities
                End Get
                Set(ByVal value As BooleanField)
                    _Deformities = value
                End Set
            End Property

            Private _Nodularity As New BooleanField
            <DataMember()>
            Public Property Nodularity() As BooleanField
                Get
                    Return _Nodularity
                End Get
                Set(ByVal value As BooleanField)
                    _Nodularity = value
                End Set
            End Property

            Private _Tenderness As New BooleanField
            <DataMember()>
            Public Property Tenderness() As BooleanField
                Get
                    Return _Tenderness
                End Get
                Set(ByVal value As BooleanField)
                    _Tenderness = value
                End Set
            End Property

            Private _Enlargement As New BooleanField
            <DataMember()>
            Public Property Enlargement() As BooleanField
                Get
                    Return _Enlargement
                End Get
                Set(ByVal value As BooleanField)
                    _Enlargement = value
                End Set
            End Property

            Private _Hemorrhoids As New BooleanField
            <DataMember()>
            Public Property Hemorrhoids() As BooleanField
                Get
                    Return _Hemorrhoids
                End Get
                Set(ByVal value As BooleanField)
                    _Hemorrhoids = value
                End Set
            End Property

            Private _Prolapse As New BooleanField
            <DataMember()>
            Public Property Prolapse() As BooleanField
                Get
                    Return _Prolapse
                End Get
                Set(ByVal value As BooleanField)
                    _Prolapse = value
                End Set
            End Property

            Private _EstrogenEffect As New BooleanField
            <DataMember()>
            Public Property EstrogenEffect() As BooleanField
                Get
                    Return _EstrogenEffect
                End Get
                Set(ByVal value As BooleanField)
                    _EstrogenEffect = value
                End Set
            End Property

            Private _PelvicSupport As New BooleanField
            <DataMember()>
            Public Property PelvicSupport() As BooleanField
                Get
                    Return _PelvicSupport
                End Get
                Set(ByVal value As BooleanField)
                    _PelvicSupport = value
                End Set
            End Property

            Private _Cystocele As New BooleanField
            <DataMember()>
            Public Property Cystocele() As BooleanField
                Get
                    Return _Cystocele
                End Get
                Set(ByVal value As BooleanField)
                    _Cystocele = value
                End Set
            End Property

            Private _Rectocele As New BooleanField
            <DataMember()>
            Public Property Rectocele() As BooleanField
                Get
                    Return _Rectocele
                End Get
                Set(ByVal value As BooleanField)
                    _Rectocele = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            Private _Urostomy As New BooleanField
            <DataMember()>
            Public Property Urostomy() As BooleanField
                Get
                    Return _Urostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Urostomy = value
                End Set
            End Property

            Private _FoleyCatheterUse As New BooleanField
            <DataMember()>
            Public Property FoleyCatheterUse() As BooleanField
                Get
                    Return _FoleyCatheterUse
                End Get
                Set(ByVal value As BooleanField)
                    _FoleyCatheterUse = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class MusculoskeletalOptionSection

            Private _AbormalGait As New BooleanField
            <DataMember()>
            Public Property AbnormalGait() As BooleanField
                Get
                    Return _AbormalGait
                End Get
                Set(ByVal value As BooleanField)
                    _AbormalGait = value
                End Set
            End Property

            Private _ClubbingNails As New BooleanField
            <DataMember()>
            Public Property ClubbingNails() As BooleanField
                Get
                    Return _ClubbingNails
                End Get
                Set(ByVal value As BooleanField)
                    _ClubbingNails = value
                End Set
            End Property

            Private _CyanosisDigits As New BooleanField
            <DataMember()>
            Public Property CyanosisDigits() As BooleanField
                Get
                    Return _CyanosisDigits
                End Get
                Set(ByVal value As BooleanField)
                    _CyanosisDigits = value
                End Set
            End Property

            Private _UpperExtremitiesAsymmetry As New BooleanField
            <DataMember()>
            Public Property UpperExtremitiesAsymmetry() As BooleanField
                Get
                    Return _UpperExtremitiesAsymmetry
                End Get
                Set(ByVal value As BooleanField)
                    _UpperExtremitiesAsymmetry = value
                End Set
            End Property

            Private _LowerExtremitiesAsymmetry As New BooleanField
            <DataMember()>
            Public Property LowerExtremitiesAsymmetry() As BooleanField
                Get
                    Return _LowerExtremitiesAsymmetry
                End Get
                Set(ByVal value As BooleanField)
                    _LowerExtremitiesAsymmetry = value
                End Set
            End Property

            Private _Dislocation As New BooleanField
            <DataMember()>
            Public Property Dislocation() As BooleanField
                Get
                    Return _Dislocation
                End Get
                Set(ByVal value As BooleanField)
                    _Dislocation = value
                End Set
            End Property

            Private _DislocationNotes As New StringField
            <DataMember()>
            Public Property DislocationNotes() As StringField
                Get
                    Return _DislocationNotes
                End Get
                Set(ByVal value As StringField)
                    _DislocationNotes = value
                End Set
            End Property

            Private _AbnormalMuscleStrengthTone As New BooleanField
            <DataMember()>
            Public Property AbnormalMuscleStrengthTone() As BooleanField
                Get
                    Return _AbnormalMuscleStrengthTone
                End Get
                Set(ByVal value As BooleanField)
                    _AbnormalMuscleStrengthTone = value
                End Set
            End Property

            Private _Flaccid As New BooleanField
            <DataMember()>
            Public Property Flaccid() As BooleanField
                Get
                    Return _Flaccid
                End Get
                Set(ByVal value As BooleanField)
                    _Flaccid = value
                End Set
            End Property

            Private _CogWheel As New BooleanField
            <DataMember()>
            Public Property CogWheel() As BooleanField
                Get
                    Return _CogWheel
                End Get
                Set(ByVal value As BooleanField)
                    _CogWheel = value
                End Set
            End Property

            Private _Spastic As New BooleanField
            <DataMember()>
            Public Property Spastic() As BooleanField
                Get
                    Return _Spastic
                End Get
                Set(ByVal value As BooleanField)
                    _Spastic = value
                End Set
            End Property

            Private _AbormalMovements As New BooleanField
            <DataMember()>
            Public Property AbnormalMovements() As BooleanField
                Get
                    Return _AbormalMovements
                End Get
                Set(ByVal value As BooleanField)
                    _AbormalMovements = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property



            '2025
            Private _NoDeformitiesOrDeformations As New BooleanField
            <DataMember()>
            Public Property NoDeformitiesOrDeformations() As BooleanField
                Get
                    Return _NoDeformitiesOrDeformations
                End Get
                Set(ByVal value As BooleanField)
                    _NoDeformitiesOrDeformations = value
                End Set
            End Property

            Private _NormalGait As New BooleanField
            <DataMember()>
            Public Property NormalGait() As BooleanField
                Get
                    Return _NormalGait
                End Get
                Set(ByVal value As BooleanField)
                    _NormalGait = value
                End Set
            End Property

            Private _AdequateROM As New BooleanField
            <DataMember()>
            Public Property AdequateROM() As BooleanField
                Get
                    Return _AdequateROM
                End Get
                Set(ByVal value As BooleanField)
                    _AdequateROM = value
                End Set
            End Property

            Private _InadequateROM As New BooleanField
            <DataMember()>
            Public Property InadequateROM() As BooleanField
                Get
                    Return _InadequateROM
                End Get
                Set(ByVal value As BooleanField)
                    _InadequateROM = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class SkinOptionSection

            Private _Rashes As New BooleanField
            <DataMember()>
            Public Property Rashes() As BooleanField
                Get
                    Return _Rashes
                End Get
                Set(ByVal value As BooleanField)
                    _Rashes = value
                End Set
            End Property

            Private _Lesions As New BooleanField
            <DataMember()>
            Public Property Lesions() As BooleanField
                Get
                    Return _Lesions
                End Get
                Set(ByVal value As BooleanField)
                    _Lesions = value
                End Set
            End Property

            Private _Ulcers As New BooleanField
            <DataMember()>
            Public Property Ulcers() As BooleanField
                Get
                    Return _Ulcers
                End Get
                Set(ByVal value As BooleanField)
                    _Ulcers = value
                End Set
            End Property

            Private _Nodules As New BooleanField
            <DataMember()>
            Public Property Nodules() As BooleanField
                Get
                    Return _Nodules
                End Get
                Set(ByVal value As BooleanField)
                    _Nodules = value
                End Set
            End Property

            Private _Induration As New BooleanField
            <DataMember()>
            Public Property Induration() As BooleanField
                Get
                    Return _Induration
                End Get
                Set(ByVal value As BooleanField)
                    _Induration = value
                End Set
            End Property

            Private _Tightening As New BooleanField
            <DataMember()>
            Public Property Tightening() As BooleanField
                Get
                    Return _Tightening
                End Get
                Set(ByVal value As BooleanField)
                    _Tightening = value
                End Set
            End Property

            Private _PurpuricLesionsNoted As New BooleanField
            <DataMember()>
            Public Property PurpuricLesionsNoted() As BooleanField
                Get
                    Return _PurpuricLesionsNoted
                End Get
                Set(ByVal value As BooleanField)
                    _PurpuricLesionsNoted = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class PsychiatricNeurologicOptionSection

            Private _CranialNervesWithDeficits As New BooleanField
            <DataMember()>
            Public Property CranialNervesWithDeficits() As BooleanField
                Get
                    Return _CranialNervesWithDeficits
                End Get
                Set(ByVal value As BooleanField)
                    _CranialNervesWithDeficits = value
                End Set
            End Property

            Private _Babinski As New BooleanField
            <DataMember()>
            Public Property Babinsky() As BooleanField
                Get
                    Return _Babinski
                End Get
                Set(ByVal value As BooleanField)
                    _Babinski = value
                End Set
            End Property

            Private _SensationByTouch As New BooleanField
            <DataMember()>
            Public Property SensationByTouch() As BooleanField
                Get
                    Return _SensationByTouch
                End Get
                Set(ByVal value As BooleanField)
                    _SensationByTouch = value
                End Set
            End Property

            Private _NoSensationTouchLegs As New BooleanField
            <DataMember()>
            Public Property NoSensationTouchLegs() As BooleanField
                Get
                    Return _NoSensationTouchLegs
                End Get
                Set(ByVal value As BooleanField)
                    _NoSensationTouchLegs = value
                End Set
            End Property

            Private _OrientedToTime As New BooleanField
            <DataMember()>
            Public Property OrientedToTime() As BooleanField
                Get
                    Return _OrientedToTime
                End Get
                Set(ByVal value As BooleanField)
                    _OrientedToTime = value
                End Set
            End Property

            Private _PlaceAndPerson As New BooleanField
            <DataMember()>
            Public Property PlaceAndPerson() As BooleanField
                Get
                    Return _PlaceAndPerson
                End Get
                Set(ByVal value As BooleanField)
                    _PlaceAndPerson = value
                End Set
            End Property

            Private _DepressedMode As New BooleanField
            <DataMember()>
            Public Property DepressedMode() As BooleanField
                Get
                    Return _DepressedMode
                End Get
                Set(ByVal value As BooleanField)
                    _DepressedMode = value
                End Set
            End Property

            Private _Anxiety As New BooleanField
            <DataMember()>
            Public Property Anxiety() As BooleanField
                Get
                    Return _Anxiety
                End Get
                Set(ByVal value As BooleanField)
                    _Anxiety = value
                End Set
            End Property

            Private _Agitation As New BooleanField
            <DataMember()>
            Public Property Agitation() As BooleanField
                Get
                    Return _Agitation
                End Get
                Set(ByVal value As BooleanField)
                    _Agitation = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            <DataMember()>
            Public Property Hemiplejia As New BooleanField
            <DataMember()>
            Public Property Cuadriplejia As New BooleanField
            <DataMember()>
            Public Property Paraplejia As New BooleanField

            '2025
            Private _AmbulatingWOLimitation As New BooleanField
            <DataMember()>
            Public Property AmbulatingWOLimitation() As BooleanField
                Get
                    Return _AmbulatingWOLimitation
                End Get
                Set(ByVal value As BooleanField)
                    _AmbulatingWOLimitation = value
                End Set
            End Property

            Private _NormalMuscleStrengthTone As New BooleanField
            <DataMember()>
            Public Property NormalMuscleStrengthTone() As BooleanField
                Get
                    Return _NormalMuscleStrengthTone
                End Get
                Set(ByVal value As BooleanField)
                    _NormalMuscleStrengthTone = value
                End Set
            End Property

            Private _AbnormalMuscleStrengthTone As New BooleanField
            <DataMember()>
            Public Property AbnormalMuscleStrengthTone() As BooleanField
                Get
                    Return _AbnormalMuscleStrengthTone
                End Get
                Set(ByVal value As BooleanField)
                    _AbnormalMuscleStrengthTone = value
                End Set
            End Property

            Private _FocalDeficits As New BooleanField
            <DataMember()>
            Public Property FocalDeficits() As BooleanField
                Get
                    Return _FocalDeficits
                End Get
                Set(ByVal value As BooleanField)
                    _FocalDeficits = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class HematologicLymphaticImmunologicOptionsSection

            Private _LymphNodes As New BooleanField
            <DataMember()>
            Public Property LymphNodes() As BooleanField
                Get
                    Return _LymphNodes
                End Get
                Set(ByVal value As BooleanField)
                    _LymphNodes = value
                End Set
            End Property

            Private _LymphNodesNotes As New StringField
            <DataMember()>
            Public Property LymphNodesNotes() As StringField
                Get
                    Return _LymphNodesNotes
                End Get
                Set(ByVal value As StringField)
                    _LymphNodesNotes = value
                End Set
            End Property

            Private _WNL As New BooleanField
            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

        End Class

#End Region

#Region " Cognitive Assessment"

        <Serializable(), DataContract()>
        Public Class CognitiveAssesmentSection
            Inherits AHASectionErrorSummary

            Private _DayOfTheWeek As New BooleanField
            Private _MonthOfTheYear As New BooleanField
            Private _Year As New BooleanField

            Private _Ball As New BooleanField
            Private _Flag As New BooleanField
            Private _Tree As New BooleanField

            Private _WNL As New BooleanField

            Private _Diagnosis As New StringField
            Private _PlanGoalsTreatmentInterventionFollowUp As New StringField

            'Public Sub New()

            '    _Diagnosis = ""
            '    _PlanGoalsTreatmentInterventionFollowUp = ""

            'End Sub

            'Private _Time As New BooleanField
            '<DataMember()> _
            'Public Property Time() As BooleanField
            '    Get
            '        Return _Time
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _Time = value
            '    End Set
            'End Property

            'Private _Place As New BooleanField
            '<DataMember()> _
            'Public Property Place() As BooleanField
            '    Get
            '        Return _Place
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _Place = value
            '    End Set
            'End Property

            'Private _Person As New BooleanField
            '<DataMember()> _
            'Public Property Person() As BooleanField
            '    Get
            '        Return _Person
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _Person = value
            '    End Set
            'End Property


            <DataMember()>
            Public Property DayOfTheWeek() As BooleanField
                Get
                    Return _DayOfTheWeek
                End Get
                Set(ByVal value As BooleanField)
                    _DayOfTheWeek = value
                End Set
            End Property

            <DataMember()>
            Public Property MonthOfTheYear() As BooleanField
                Get
                    Return _MonthOfTheYear
                End Get
                Set(ByVal value As BooleanField)
                    _MonthOfTheYear = value
                End Set
            End Property

            <DataMember()>
            Public Property Year() As BooleanField
                Get
                    Return _Year
                End Get
                Set(ByVal value As BooleanField)
                    _Year = value
                End Set
            End Property

            <DataMember()>
            Public Property Ball() As BooleanField
                Get
                    Return _Ball
                End Get
                Set(ByVal value As BooleanField)
                    _Ball = value
                End Set
            End Property

            <DataMember()>
            Public Property Flag() As BooleanField
                Get
                    Return _Flag
                End Get
                Set(ByVal value As BooleanField)
                    _Flag = value
                End Set
            End Property

            <DataMember()>
            Public Property Tree() As BooleanField
                Get
                    Return _Tree
                End Get
                Set(ByVal value As BooleanField)
                    _Tree = value
                End Set
            End Property

            <DataMember()>
            Public Property WNL() As BooleanField
                Get
                    Return _WNL
                End Get
                Set(ByVal value As BooleanField)
                    _WNL = value
                End Set
            End Property

            <DataMember()>
            Public Property Diagnosis() As StringField
                Get
                    Return _Diagnosis
                End Get
                Set(ByVal value As StringField)
                    _Diagnosis = value
                End Set
            End Property

            <DataMember()>
            Public Property PlanGoalsTreatmentInterventionFollowUp() As StringField
                Get
                    Return _PlanGoalsTreatmentInterventionFollowUp
                End Get
                Set(ByVal value As StringField)
                    _PlanGoalsTreatmentInterventionFollowUp = value
                End Set
            End Property

        End Class

#End Region

#Region " Pain Screening"
        <Serializable(), DataContract()>
        Public Class PainScreeningSection
            Inherits AHASectionErrorSummary

            Private _PatientHaveComplaint As New BooleanField
            Private _PainIsLocated As New StringField
            Private _RatePainExperiencingNow As New IntegerField
            Private _ManagePainWith As New StringField
            Private _TreatmentHaveBeenEffective As New BooleanField

            Private _NA As New BooleanField
            Private _BathingDressing As New BooleanField
            Private _Mood As New BooleanField
            Private _WalkingAbility As New BooleanField
            Private _Employment As New BooleanField
            Private _HouseWork As New BooleanField
            Private _Sleep As New BooleanField
            Private _RelationshipWithOther As New BooleanField
            Private _EnjoymentOfLife As New BooleanField
            Private _Transportation As New BooleanField
            Private _Toileting As New BooleanField
            Private _FoodPreparation As New BooleanField

            Private _PainDueTo As New StringField
            Private _PlanGoalsTreatmentInterventionFollowUp As New StringField

            'Public Sub New()

            '    _PainIsLocated = ""
            '    _ManagePainWith = ""
            '    _PainDueTo = ""
            '    _PlanGoalsTreatmentInterventionFollowUp = ""

            'End Sub

            <DataMember()>
            Public Property PatientHaveComplaint() As BooleanField
                Get
                    Return _PatientHaveComplaint
                End Get
                Set(ByVal value As BooleanField)
                    _PatientHaveComplaint = value
                End Set
            End Property

            <DataMember()>
            Public Property PainIsLocated() As StringField
                Get
                    Return _PainIsLocated
                End Get
                Set(ByVal value As StringField)
                    _PainIsLocated = value
                End Set
            End Property

            <DataMember()>
            Public Property RatePainExperiencingNow() As IntegerField
                Get
                    Return _RatePainExperiencingNow
                End Get
                Set(ByVal value As IntegerField)
                    _RatePainExperiencingNow = value
                End Set
            End Property

            <DataMember()>
            Public Property ManagePainWith() As StringField
                Get
                    Return _ManagePainWith
                End Get
                Set(ByVal value As StringField)
                    _ManagePainWith = value
                End Set
            End Property

            <DataMember()>
            Public Property TreatmentHaveBeenEffective() As BooleanField
                Get
                    Return _TreatmentHaveBeenEffective
                End Get
                Set(ByVal value As BooleanField)
                    _TreatmentHaveBeenEffective = value
                End Set
            End Property

            Private _TreatmentHaveBeenEffectiveNA As New BooleanField
            <DataMember()>
            Public Property TreatmentHaveBeenEffectiveNA() As BooleanField
                Get
                    Return _TreatmentHaveBeenEffectiveNA
                End Get
                Set(ByVal value As BooleanField)
                    _TreatmentHaveBeenEffectiveNA = value
                End Set
            End Property

            <DataMember()>
            Public Property NA() As BooleanField
                Get
                    Return _NA
                End Get
                Set(ByVal value As BooleanField)
                    _NA = value
                End Set
            End Property

            <DataMember()>
            Public Property BathingDressing() As BooleanField
                Get
                    Return _BathingDressing
                End Get
                Set(ByVal value As BooleanField)
                    _BathingDressing = value
                End Set
            End Property

            <DataMember()>
            Public Property Mood() As BooleanField
                Get
                    Return _Mood
                End Get
                Set(ByVal value As BooleanField)
                    _Mood = value
                End Set
            End Property

            <DataMember()>
            Public Property WalkingAbility() As BooleanField
                Get
                    Return _WalkingAbility
                End Get
                Set(ByVal value As BooleanField)
                    _WalkingAbility = value
                End Set
            End Property

            <DataMember()>
            Public Property Employment() As BooleanField
                Get
                    Return _Employment
                End Get
                Set(ByVal value As BooleanField)
                    _Employment = value
                End Set
            End Property

            <DataMember()>
            Public Property HouseWork() As BooleanField
                Get
                    Return _HouseWork
                End Get
                Set(ByVal value As BooleanField)
                    _HouseWork = value
                End Set
            End Property

            <DataMember()>
            Public Property Sleep() As BooleanField
                Get
                    Return _Sleep
                End Get
                Set(ByVal value As BooleanField)
                    _Sleep = value
                End Set
            End Property

            <DataMember()>
            Public Property RelationshipWithOther() As BooleanField
                Get
                    Return _RelationshipWithOther
                End Get
                Set(ByVal value As BooleanField)
                    _RelationshipWithOther = value
                End Set
            End Property

            <DataMember()>
            Public Property EnjoymentOfLife() As BooleanField
                Get
                    Return _EnjoymentOfLife
                End Get
                Set(ByVal value As BooleanField)
                    _EnjoymentOfLife = value
                End Set
            End Property

            <DataMember()>
            Public Property Transportation() As BooleanField
                Get
                    Return _Transportation
                End Get
                Set(ByVal value As BooleanField)
                    _Transportation = value
                End Set
            End Property

            <DataMember()>
            Public Property Toileting() As BooleanField
                Get
                    Return _Toileting
                End Get
                Set(ByVal value As BooleanField)
                    _Toileting = value
                End Set
            End Property

            <DataMember()>
            Public Property FoodPreparation() As BooleanField
                Get
                    Return _FoodPreparation
                End Get
                Set(ByVal value As BooleanField)
                    _FoodPreparation = value
                End Set
            End Property

            <DataMember()>
            Public Property PainDueTo() As StringField
                Get
                    Return _PainDueTo
                End Get
                Set(ByVal value As StringField)
                    _PainDueTo = value
                End Set
            End Property

            <DataMember()>
            Public Property PlanGoalsTreatmentInterventaionFollowUp() As StringField
                Get
                    Return _PlanGoalsTreatmentInterventionFollowUp
                End Get
                Set(ByVal value As StringField)
                    _PlanGoalsTreatmentInterventionFollowUp = value
                End Set
            End Property

            Private _Others As New StringField
            <DataMember()>
            Public Property Others() As StringField
                Get
                    Return _Others
                End Get
                Set(ByVal value As StringField)
                    _Others = value
                End Set
            End Property

            Private _CausalCondition As StringField
            <DataMember()>
            Public Property CausalCondition() As StringField
                Get
                    Return _CausalCondition
                End Get
                Set(ByVal value As StringField)
                    _CausalCondition = value
                End Set
            End Property

            Private _ArthritisDueToInfection As New BooleanField
            <DataMember()>
            Public Property ArthritisDueToInfection() As BooleanField
                Get
                    Return _ArthritisDueToInfection
                End Get
                Set(ByVal value As BooleanField)
                    _ArthritisDueToInfection = value
                End Set
            End Property

            Private _PainEvaluationOtherCondition As New BooleanField
            <DataMember()>
            Public Property PainEvaluationOtherCondition() As BooleanField
                Get
                    Return _PainEvaluationOtherCondition
                End Get
                Set(ByVal value As BooleanField)
                    _PainEvaluationOtherCondition = value
                End Set
            End Property

            Private _PainEvaluationOtherConditionText As New StringField
            <DataMember()>
            Public Property PainEvaluationOtherConditionText() As StringField
                Get
                    Return _PainEvaluationOtherConditionText
                End Get
                Set(ByVal value As StringField)
                    _PainEvaluationOtherConditionText = value
                End Set
            End Property
            <DataMember()>
            Public Property PainEvaluationOtherActivities As BooleanField
        End Class

#End Region

#Region " Activities of Daily Living"

        <Serializable(), DataContract()>
        Public Class ActivitiesOfDailyLivingSection
            Inherits AHASectionErrorSummary

            Private _Bathing As New BooleanField
            Private _BathingComments As New StringField

            Private _DressingAndUndressing As New BooleanField
            Private _DressingAndUndressingComments As New StringField

            Private _Eating As New BooleanField
            Private _EatingComments As New StringField

            Private _TransferringBedChair As New BooleanField
            Private _TransferringBedChairComments As New StringField

            Private _VoluntarilyControl As New BooleanField
            Private _VoluntarilyControlComments As New StringField

            Private _UsingToilet As New BooleanField
            Private _UsingToiletComments As New StringField

            Private _Walking As New BooleanField
            Private _WalkingComments As New StringField

            Private _HistoryOfFalling As New BooleanField

            Private _HistoryOfFalling_Comments As New StringField

            Private _DependenceOnRespirator As New BooleanField

            Private _DependenceOnWheelchair As New BooleanField

            Private _DependenceOnOxygen As New BooleanField




            'Public Sub New()

            '    _BathingComments = ""
            '    _DressingAndUndressingComments = ""
            '    _EatingComments = ""
            '    _TransferringBedChairComments = ""
            '    _VoluntarilyControlComments = ""
            '    _UsingToiletComments = ""
            '    _WalkingComments = ""

            'End Sub

            <DataMember()>
            Public Property Bathing() As BooleanField
                Get
                    Return _Bathing
                End Get
                Set(ByVal value As BooleanField)
                    _Bathing = value
                End Set
            End Property

            <DataMember()>
            Public Property BathingComments() As StringField
                Get
                    Return _BathingComments
                End Get
                Set(ByVal value As StringField)
                    _BathingComments = value
                End Set
            End Property

            <DataMember()>
            Public Property DressingAndUndressing() As BooleanField
                Get
                    Return _DressingAndUndressing
                End Get
                Set(ByVal value As BooleanField)
                    _DressingAndUndressing = value
                End Set
            End Property

            <DataMember()>
            Public Property DressingAndUndressingComments() As StringField
                Get
                    Return _DressingAndUndressingComments
                End Get
                Set(ByVal value As StringField)
                    _DressingAndUndressingComments = value
                End Set
            End Property

            <DataMember()>
            Public Property Eating() As BooleanField
                Get
                    Return _Eating
                End Get
                Set(ByVal value As BooleanField)
                    _Eating = value
                End Set
            End Property

            <DataMember()>
            Public Property EatingComments() As StringField
                Get
                    Return _EatingComments
                End Get
                Set(ByVal value As StringField)
                    _EatingComments = value
                End Set
            End Property

            <DataMember()>
            Public Property TransferringBedChair() As BooleanField
                Get
                    Return _TransferringBedChair
                End Get
                Set(ByVal value As BooleanField)
                    _TransferringBedChair = value
                End Set
            End Property

            <DataMember()>
            Public Property TransferringBedChairComments() As StringField
                Get
                    Return _TransferringBedChairComments
                End Get
                Set(ByVal value As StringField)
                    _TransferringBedChairComments = value
                End Set
            End Property

            <DataMember()>
            Public Property VoluntarilyControl() As BooleanField
                Get
                    Return _VoluntarilyControl
                End Get
                Set(ByVal value As BooleanField)
                    _VoluntarilyControl = value
                End Set
            End Property

            <DataMember()>
            Public Property VoluntarilyControlComments() As StringField
                Get
                    Return _VoluntarilyControlComments
                End Get
                Set(ByVal value As StringField)
                    _VoluntarilyControlComments = value
                End Set
            End Property

            <DataMember()>
            Public Property UsingToilet() As BooleanField
                Get
                    Return _UsingToilet
                End Get
                Set(ByVal value As BooleanField)
                    _UsingToilet = value
                End Set
            End Property

            <DataMember()>
            Public Property UsingToiletComments() As StringField
                Get
                    Return _UsingToiletComments
                End Get
                Set(ByVal value As StringField)
                    _UsingToiletComments = value
                End Set
            End Property

            <DataMember()>
            Public Property Walking() As BooleanField
                Get
                    Return _Walking
                End Get
                Set(ByVal value As BooleanField)
                    _Walking = value
                End Set
            End Property

            <DataMember()>
            Public Property WalkingComments() As StringField
                Get
                    Return _WalkingComments
                End Get
                Set(ByVal value As StringField)
                    _WalkingComments = value
                End Set
            End Property

            Private _BedFast As New BooleanField
            <DataMember()>
            Public Property BedFast() As BooleanField
                Get
                    Return _BedFast
                End Get
                Set(ByVal value As BooleanField)
                    _BedFast = value
                End Set
            End Property

            <DataMember()>
            Public Property HistoryOfFalling() As BooleanField
                Get
                    Return _HistoryOfFalling
                End Get
                Set(ByVal value As BooleanField)
                    _HistoryOfFalling = value
                End Set
            End Property

            <DataMember()>
            Public Property HistoryOfFalling_Comments() As StringField
                Get
                    Return _HistoryOfFalling_Comments
                End Get
                Set(ByVal value As StringField)
                    _HistoryOfFalling_Comments = value
                End Set
            End Property

            <DataMember()>
            Public Property DependenceOnRespirator() As BooleanField
                Get
                    Return _DependenceOnRespirator
                End Get
                Set(ByVal value As BooleanField)
                    _DependenceOnRespirator = value
                End Set
            End Property

            <DataMember()>
            Public Property DependenceOnWheelchair() As BooleanField
                Get
                    Return _DependenceOnWheelchair
                End Get
                Set(ByVal value As BooleanField)
                    _DependenceOnWheelchair = value
                End Set
            End Property

            <DataMember()>
            Public Property DependenceOnOxygen() As BooleanField
                Get
                    Return _DependenceOnOxygen
                End Get
                Set(ByVal value As BooleanField)
                    _DependenceOnOxygen = value
                End Set
            End Property
        End Class

#End Region

#Region " Screnning Schedule / FollowUp"

        <Serializable(), DataContract()>
        Public Class ScreeningScheduleSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                MyBase.New()
                Me.ZosterVaccineOrdered = New BooleanField
                Me.ZosterVaccineRefuse = New BooleanField
                Me.ZosterVaccineShot = New IntegerField
                Me.ZosterVaccineShotDate1 = New DateField
                Me.ZosterVaccineShotDate2 = New DateField
                Me.COVID19VaccineHouse = New StringField
                Me.COVID19VaccineOrdered = New BooleanField
                Me.COVID19VaccineRefuse = New BooleanField
                Me.COVID19VaccineShot = New IntegerField
                Me.COVID19VaccineShotDate1 = New DateField
                Me.COVID19VaccineShotDate2 = New DateField
                Me.COVID19VaccineShotDate3 = New DateField
                Me.Screening_Eye_Severity = New BooleanField
                Me.Screening_Eye_SeverityLevel = New IntegerField
                Me.Screening_Retinopathy_Eye = New IntegerField
                Me.Screening_Proliferative_Eye = New IntegerField
                Me.MacularEdema = New BooleanField
                Me.MacularEdema_Eye = New IntegerField
            End Sub



            Private _ZosterVaccineShotDate2 As DateField
            <DataMember()>
            Public Property ZosterVaccineShotDate2() As DateField
                Get
                    Return _ZosterVaccineShotDate2
                End Get
                Set(ByVal value As DateField)
                    _ZosterVaccineShotDate2 = value
                End Set
            End Property



            Private _ZosterVaccineShotDate1 As DateField
            <DataMember()>
            Public Property ZosterVaccineShotDate1() As DateField
                Get
                    Return _ZosterVaccineShotDate1
                End Get
                Set(ByVal value As DateField)
                    _ZosterVaccineShotDate1 = value
                End Set
            End Property





            Private _ZosterVaccineRefuse As BooleanField
            <DataMember()>
            Public Property ZosterVaccineRefuse() As BooleanField
                Get
                    Return _ZosterVaccineRefuse
                End Get
                Set(ByVal value As BooleanField)
                    _ZosterVaccineRefuse = value
                End Set
            End Property




            Private _ZosterVaccineOrdered As BooleanField
            <DataMember()>
            Public Property ZosterVaccineOrdered() As BooleanField
                Get
                    Return _ZosterVaccineOrdered
                End Get
                Set(ByVal value As BooleanField)
                    _ZosterVaccineOrdered = value
                End Set
            End Property




            Private _ZosterVaccineShot As IntegerField
            <DataMember()>
            Public Property ZosterVaccineShot() As IntegerField
                Get
                    Return _ZosterVaccineShot
                End Get
                Set(ByVal value As IntegerField)
                    _ZosterVaccineShot = value
                End Set
            End Property





            Private _COVID19VaccineHouse As StringField
            <DataMember()>
            Public Property COVID19VaccineHouse() As StringField
                Get
                    Return _COVID19VaccineHouse
                End Get
                Set(ByVal value As StringField)
                    _COVID19VaccineHouse = value
                End Set
            End Property



            Private _COVID19VaccineShot As IntegerField
            <DataMember()>
            Public Property COVID19VaccineShot() As IntegerField
                Get
                    Return _COVID19VaccineShot
                End Get
                Set(ByVal value As IntegerField)
                    _COVID19VaccineShot = value
                End Set
            End Property


            Private _COVID19VaccineShotDate1 As DateField
            <DataMember()>
            Public Property COVID19VaccineShotDate1() As DateField
                Get
                    Return _COVID19VaccineShotDate1
                End Get
                Set(ByVal value As DateField)
                    _COVID19VaccineShotDate1 = value
                End Set
            End Property

            Private _COVID19VaccineShotDate2 As DateField
            <DataMember()>
            Public Property COVID19VaccineShotDate2() As DateField
                Get
                    Return _COVID19VaccineShotDate2
                End Get
                Set(ByVal value As DateField)
                    _COVID19VaccineShotDate2 = value
                End Set
            End Property

            Private _COVID19VaccineShotDate3 As DateField
            <DataMember()>
            Public Property COVID19VaccineShotDate3() As DateField
                Get
                    Return _COVID19VaccineShotDate3
                End Get
                Set(ByVal value As DateField)
                    _COVID19VaccineShotDate3 = value
                End Set
            End Property

            Private _COVID19VaccineRefuse As BooleanField
            <DataMember()>
            Public Property COVID19VaccineRefuse() As BooleanField
                Get
                    Return _COVID19VaccineRefuse
                End Get
                Set(ByVal value As BooleanField)
                    _COVID19VaccineRefuse = value
                End Set
            End Property

            Private _COVID19VaccineOrdered As BooleanField
            <DataMember()>
            Public Property COVID19VaccineOrdered() As BooleanField
                Get
                    Return _COVID19VaccineOrdered
                End Get
                Set(ByVal value As BooleanField)
                    _COVID19VaccineOrdered = value
                End Set
            End Property



            Private _IsDiabetic As New BooleanField

            <DataMember()>
            Public Property IsDiabetic() As BooleanField
                Get
                    Return _IsDiabetic
                End Get
                Set(ByVal value As BooleanField)
                    _IsDiabetic = value
                End Set
            End Property

            'Actualizacion 2020 de nuevo campos en Screening
            Private _BoneMineralDensityResult As New StringField
            <DataMember()>
            Public Property BoneMineralDensityResult() As StringField
                Get
                    Return _BoneMineralDensityResult
                End Get
                Set(ByVal value As StringField)
                    _BoneMineralDensityResult = value
                End Set
            End Property

            Private _BoneMineralDensityResult_NA As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityResult_NA() As BooleanField
                Get
                    Return _BoneMineralDensityResult_NA
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityResult_NA = value
                End Set
            End Property

            Private _BoneMineralDensityResult_Normal As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityResult_Normal() As BooleanField
                Get
                    Return _BoneMineralDensityResult_Normal
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityResult_Normal = value
                End Set
            End Property

            Private _BoneMineralDensityResult_Osteopenia As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityResult_Osteopenia() As BooleanField
                Get
                    Return _BoneMineralDensityResult_Osteopenia
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityResult_Osteopenia = value
                End Set
            End Property

            Private _BoneMineralDensityResult_Osteoporosis As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityResult_Osteoporosis() As BooleanField
                Get
                    Return _BoneMineralDensityResult_Osteoporosis
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityResult_Osteoporosis = value
                End Set
            End Property

            Private _BoneMineralDensityDate As New DateField
            <DataMember()>
            Public Property BoneMineralDensityDate() As DateField
                Get
                    Return _BoneMineralDensityDate
                End Get
                Set(ByVal value As DateField)
                    _BoneMineralDensityDate = value
                End Set
            End Property

            'Private _BoneMineralDensityReviewed As New BooleanField
            '<DataMember()> _
            'Public Property BoneMineralDensityReviewed() As BooleanField
            '    Get
            '        Return _BoneMineralDensityReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _BoneMineralDensityReviewed = value
            '    End Set
            'End Property

            Private _BoneMineralDensityNAFor As New StringField
            <DataMember()>
            Public Property BoneMineralDensityNAFor() As StringField
                Get
                    Return _BoneMineralDensityNAFor
                End Get
                Set(ByVal value As StringField)
                    _BoneMineralDensityNAFor = value
                End Set
            End Property

            Private _BoneMineralDensityPrescribed As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityPrescribed() As BooleanField
                Get
                    Return _BoneMineralDensityPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityPrescribed = value
                End Set
            End Property

            Private _BoneMineralDensityRxOrdered As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityRxOrdered() As BooleanField
                Get
                    Return _BoneMineralDensityRxOrdered
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityRxOrdered = value
                End Set
            End Property

            Private _CardiovascularLDLDate As New DateField
            <DataMember()>
            Public Property CardiovascularLDLDate() As DateField
                Get
                    Return _CardiovascularLDLDate
                End Get
                Set(ByVal value As DateField)
                    _CardiovascularLDLDate = value
                End Set
            End Property

            Private _CardiovascularLDLResult As New StringField
            <DataMember()>
            Public Property CardiovascularLDLResult() As StringField
                Get
                    Return _CardiovascularLDLResult
                End Get
                Set(ByVal value As StringField)
                    _CardiovascularLDLResult = value
                End Set
            End Property

            'Private _CardiovascularLDLReviewed As New BooleanField
            '<DataMember()> _
            'Public Property CardiovascularLDLReviewed() As BooleanField
            '    Get
            '        Return _CardiovascularLDLReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _CardiovascularLDLReviewed = value
            '    End Set
            'End Property

            Private _CardiovascularLDLNAFor As New StringField
            <DataMember()>
            Public Property CardiovascularLDLNAFor() As StringField
                Get
                    Return _CardiovascularLDLNAFor
                End Get
                Set(ByVal value As StringField)
                    _CardiovascularLDLNAFor = value
                End Set
            End Property

            Private _CardiovascularLDLPrescribed As New BooleanField
            <DataMember()>
            Public Property CardiovascularLDLPrescribed() As BooleanField
                Get
                    Return _CardiovascularLDLPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _CardiovascularLDLPrescribed = value
                End Set
            End Property


            Private _CardiovascularBetaDate As New DateField
            <DataMember()>
            Public Property CardiovascularBetaDate() As DateField
                Get
                    Return _CardiovascularBetaDate
                End Get
                Set(ByVal value As DateField)
                    _CardiovascularBetaDate = value
                End Set
            End Property

            Private _CardiovascularBetaResult As New StringField
            <DataMember()>
            Public Property CardiovascularBetaResult() As StringField
                Get
                    Return _CardiovascularBetaResult
                End Get
                Set(ByVal value As StringField)
                    _CardiovascularBetaResult = value
                End Set
            End Property

            'Private _CardiovascularBetaReviewed As New BooleanField
            '<DataMember()> _
            'Public Property CardiovascularBetaReviewed() As BooleanField
            '    Get
            '        Return _CardiovascularBetaReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _CardiovascularBetaReviewed = value
            '    End Set
            'End Property

            Private _CardiovascularBetaNAFor As New StringField
            <DataMember()>
            Public Property CardiovascularBetaNAFor() As StringField
                Get
                    Return _CardiovascularBetaNAFor
                End Get
                Set(ByVal value As StringField)
                    _CardiovascularBetaNAFor = value
                End Set
            End Property


            Private _CardiovascularBetaPrescribed As New BooleanField
            <DataMember()>
            Public Property CardiovascularBetaPrescribed() As BooleanField
                Get
                    Return _CardiovascularBetaPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _CardiovascularBetaPrescribed = value
                End Set
            End Property


            Private _ColorectalCancerScreeningSelectedIndex As New IntegerField
            <DataMember()>
            Public Property ColorectalCancerScreeningSelectedIndex() As IntegerField
                Get
                    Return _ColorectalCancerScreeningSelectedIndex
                End Get
                Set(ByVal value As IntegerField)
                    _ColorectalCancerScreeningSelectedIndex = value
                End Set
            End Property

            Private _ColorectalCancerScreeningDate As New DateField
            <DataMember()>
            Public Property ColorectalCancerScreeningDate() As DateField
                Get
                    Return _ColorectalCancerScreeningDate
                End Get
                Set(ByVal value As DateField)
                    _ColorectalCancerScreeningDate = value
                End Set
            End Property

            Private _ColorectalCancerScreeningResult As New StringField
            <DataMember()>
            Public Property ColorectalCancerScreeningResult() As StringField
                Get
                    Return _ColorectalCancerScreeningResult
                End Get
                Set(ByVal value As StringField)
                    _ColorectalCancerScreeningResult = value
                End Set
            End Property

            'Private _ColorectalCancerScreeningReviewed As New BooleanField
            '<DataMember()> _
            'Public Property ColorectalCancerScreeningReviewed() As BooleanField
            '    Get
            '        Return _ColorectalCancerScreeningReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _ColorectalCancerScreeningReviewed = value
            '    End Set
            'End Property

            Private _ColorectalCancerScreeningNAFor As New StringField
            <DataMember()>
            Public Property ColorectalCancerScreeningNAFor() As StringField
                Get
                    Return _ColorectalCancerScreeningNAFor
                End Get
                Set(ByVal value As StringField)
                    _ColorectalCancerScreeningNAFor = value
                End Set
            End Property

            Private _ColorectalCancerScreeningPrescribed As New BooleanField
            <DataMember()>
            Public Property ColorectalCancerScreeningPrescribed() As BooleanField
                Get
                    Return _ColorectalCancerScreeningPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalCancerScreeningPrescribed = value
                End Set
            End Property

            Private _ColorectalColonoscopy As New BooleanField
            <DataMember()>
            Public Property ColorectalColonoscopy() As BooleanField
                Get
                    Return _ColorectalColonoscopy
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalColonoscopy = value
                End Set
            End Property

            Private _ColorectalColonoscopyDate As New DateField
            <DataMember()>
            Public Property ColorectalColonoscopyDate() As DateField
                Get
                    Return _ColorectalColonoscopyDate
                End Get
                Set(ByVal value As DateField)
                    _ColorectalColonoscopyDate = value
                End Set
            End Property

            Private _ColorectalColonoscopyResult As New StringField
            <DataMember()>
            Public Property ColorectalColonoscopyResult() As StringField
                Get
                    Return _ColorectalColonoscopyResult
                End Get
                Set(ByVal value As StringField)
                    _ColorectalColonoscopyResult = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_NA As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_NA() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_NA
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_NA = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_Negative As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_Negative() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_Negative
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_Negative = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_Diverticles As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_Diverticles() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_Diverticles
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_Diverticles = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_BleedingAreas As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_BleedingAreas() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_BleedingAreas
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_BleedingAreas = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_CAInColon As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_CAInColon() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_CAInColon
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_CAInColon = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_CAInRectum As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_CAInRectum() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_CAInRectum
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_CAInRectum = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_Colitis As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_Colitis() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_Colitis
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_Colitis = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_UlcerativeOlitis As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_UlcerativeOlitis() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_UlcerativeOlitis
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_UlcerativeOlitis = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_CrohnsDisease As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_CrohnsDisease() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_CrohnsDisease
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_CrohnsDisease = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_Polyps As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_Polyps() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_Polyps
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_Polyps = value
                End Set
            End Property

            Private _BoneMineralDensityResult_Other As New BooleanField
            <DataMember()>
            Public Property BoneMineralDensityResult_Other() As BooleanField
                Get
                    Return _BoneMineralDensityResult_Other
                End Get
                Set(ByVal value As BooleanField)
                    _BoneMineralDensityResult_Other = value
                End Set
            End Property

            Private _Colorectal_ColonoscopyResult_Other As New BooleanField
            <DataMember()>
            Public Property Colorectal_ColonoscopyResult_Other() As BooleanField
                Get
                    Return _Colorectal_ColonoscopyResult_Other
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_ColonoscopyResult_Other = value
                End Set
            End Property

            Private _ColorectalColonoscopyNAFor As New StringField
            <DataMember()>
            Public Property ColorectalColonoscopyNAFor() As StringField
                Get
                    Return _ColorectalColonoscopyNAFor
                End Get
                Set(ByVal value As StringField)
                    _ColorectalColonoscopyNAFor = value
                End Set
            End Property

            Private _ColorectalColonoscopyPrescribe As New BooleanField
            <DataMember()>
            Public Property ColorectalColonoscopyPrescribe() As BooleanField
                Get
                    Return _ColorectalColonoscopyPrescribe
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalColonoscopyPrescribe = value
                End Set
            End Property

            Private _ColorectalOccultBlood As New BooleanField
            <DataMember()>
            Public Property ColorectalOccultBlood() As BooleanField
                Get
                    Return _ColorectalOccultBlood
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalOccultBlood = value
                End Set
            End Property

            Private _ColorectalOccultBloodDate As New DateField
            <DataMember()>
            Public Property ColorectalOccultBloodDate() As DateField
                Get
                    Return _ColorectalOccultBloodDate
                End Get
                Set(ByVal value As DateField)
                    _ColorectalOccultBloodDate = value
                End Set
            End Property

            Private _ColorectalOccultBloodResult As New StringField
            <DataMember()>
            Public Property ColorectalOccultBloodResult() As StringField
                Get
                    Return _ColorectalOccultBloodResult
                End Get
                Set(ByVal value As StringField)
                    _ColorectalOccultBloodResult = value
                End Set
            End Property

            Private _ColorectalOccultBloodNAFor As New StringField
            <DataMember()>
            Public Property ColorectalOccultBloodNAFor() As StringField
                Get
                    Return _ColorectalOccultBloodNAFor
                End Get
                Set(ByVal value As StringField)
                    _ColorectalOccultBloodNAFor = value
                End Set
            End Property

            Private _ColorectalOccultBloodPrescribe As New BooleanField
            <DataMember()>
            Public Property ColorectalOccultBloodPrescribe() As BooleanField
                Get
                    Return _ColorectalOccultBloodPrescribe
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalOccultBloodPrescribe = value
                End Set
            End Property

            Private _ColorectalFlexibleSigmoidoscopy As New BooleanField
            <DataMember()>
            Public Property ColorectalFlexibleSigmoidoscopy() As BooleanField
                Get
                    Return _ColorectalFlexibleSigmoidoscopy
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalFlexibleSigmoidoscopy = value
                End Set
            End Property

            Private _ColorectalFlexibleSigmoidoscopyDate As New DateField
            <DataMember()>
            Public Property ColorectalFlexibleSigmoidoscopyDate() As DateField
                Get
                    Return _ColorectalFlexibleSigmoidoscopyDate
                End Get
                Set(ByVal value As DateField)
                    _ColorectalFlexibleSigmoidoscopyDate = value
                End Set
            End Property

            Private _ColorectalFlexibleSigmoidoscopyResult As New StringField
            <DataMember()>
            Public Property ColorectalFlexibleSigmoidoscopyResult() As StringField
                Get
                    Return _ColorectalFlexibleSigmoidoscopyResult
                End Get
                Set(ByVal value As StringField)
                    _ColorectalFlexibleSigmoidoscopyResult = value
                End Set
            End Property


            Private _Colorectal_FlexibleSigmoidoscopyResult_NA As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_NA() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_NA
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_NA = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_Negative As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_Negative() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_Negative
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_Negative = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_AnalFissure As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_AnalFissure() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_AnalFissure
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_AnalFissure = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_CAInRectum As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_CAInRectum() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_CAInRectum
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_CAInRectum = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_Diverticles As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_Diverticles() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_Diverticles
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_Diverticles = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease = value
                End Set
            End Property

            Private _Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection As New BooleanField
            <DataMember()>
            Public Property Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection() As BooleanField
                Get
                    Return _Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection
                End Get
                Set(ByVal value As BooleanField)
                    _Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection = value
                End Set
            End Property

            Private _ColorectalFlexibleSigmoidoscopyNAFor As New StringField
            <DataMember()>
            Public Property ColorectalFlexibleSigmoidoscopyNAFor() As StringField
                Get
                    Return _ColorectalFlexibleSigmoidoscopyNAFor
                End Get
                Set(ByVal value As StringField)
                    _ColorectalFlexibleSigmoidoscopyNAFor = value
                End Set
            End Property

            Private _ColorectalFlexibleSigmoidoscopyPrescribe As New BooleanField
            <DataMember()>
            Public Property ColorectalFlexibleSigmoidoscopyPrescribe() As BooleanField
                Get
                    Return _ColorectalFlexibleSigmoidoscopyPrescribe
                End Get
                Set(ByVal value As BooleanField)
                    _ColorectalFlexibleSigmoidoscopyPrescribe = value
                End Set
            End Property

            Private _DiabetesScreening_DilatedEyeExamDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_DilatedEyeExamDate() As DateField
                Get
                    Return _DiabetesScreening_DilatedEyeExamDate
                End Get
                Set(ByVal value As DateField)
                    _DiabetesScreening_DilatedEyeExamDate = value
                End Set
            End Property

            Private _DiabetesScreening_DilatedEyeExamResult As New StringField
            <DataMember()>
            Public Property DiabetesScreening_DilatedEyeExamResult() As StringField
                Get
                    Return _DiabetesScreening_DilatedEyeExamResult
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_DilatedEyeExamResult = value
                End Set
            End Property

            'Private _DiabetesScreening_DilatedEyeExamReviewed As New BooleanField
            '<DataMember()> _
            'Public Property DiabetesScreening_DilatedEyeExamReviewed() As BooleanField
            '    Get
            '        Return _DiabetesScreening_DilatedEyeExamReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _DiabetesScreening_DilatedEyeExamReviewed = value
            '    End Set
            'End Property

            Private _DiabetesScreening_DilatedEyeExamNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_DilatedEyeExamNAFor() As StringField
                Get
                    Return _DiabetesScreening_DilatedEyeExamNAFor
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_DilatedEyeExamNAFor = value
                End Set
            End Property

            Private _DiabetesScreening_DilatedEyeExamPrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_DilatedEyeExamPrescribed() As BooleanField
                Get
                    Return _DiabetesScreening_DilatedEyeExamPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _DiabetesScreening_DilatedEyeExamPrescribed = value
                End Set
            End Property


            Private _DiabetesScreening_LDLDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_LDLDate() As DateField
                Get
                    Return _DiabetesScreening_LDLDate
                End Get
                Set(ByVal value As DateField)
                    _DiabetesScreening_LDLDate = value
                End Set
            End Property

            Private _DiabetesScreening_LDLResult As New StringField
            <DataMember()>
            Public Property DiabetesScreening_LDLResult() As StringField
                Get
                    Return _DiabetesScreening_LDLResult
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_LDLResult = value
                End Set
            End Property

            'Private _DiabetesScreening_LDLReviewed As New BooleanField
            '<DataMember()> _
            'Public Property DiabetesScreening_LDLReviewed() As BooleanField
            '    Get
            '        Return _DiabetesScreening_LDLReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _DiabetesScreening_LDLReviewed = value
            '    End Set
            'End Property

            Private _DiabetesScreening_LDLNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_LDLNAFor() As StringField
                Get
                    Return _DiabetesScreening_LDLNAFor
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_LDLNAFor = value
                End Set
            End Property

            Private _DiabetesScreening_LDLPrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_LDLPrescribed() As BooleanField
                Get
                    Return _DiabetesScreening_LDLPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _DiabetesScreening_LDLPrescribed = value
                End Set
            End Property

            Private _DiabetesScreening_HGA1C_3MDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_3MDate() As DateField
                Get
                    Return _DiabetesScreening_HGA1C_3MDate
                End Get
                Set(ByVal value As DateField)
                    _DiabetesScreening_HGA1C_3MDate = value
                End Set
            End Property

            Private _DiabetesScreening_HGA1C_3MResult As New StringField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_3MResult() As StringField
                Get
                    Return _DiabetesScreening_HGA1C_3MResult
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_HGA1C_3MResult = value
                End Set
            End Property

            'Private _DiabetesScreening_HGA1C_3MReviewed As New BooleanField
            '<DataMember()> _
            'Public Property DiabetesScreening_HGA1C_3MReviewed() As BooleanField
            '    Get
            '        Return _DiabetesScreening_HGA1C_3MReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _DiabetesScreening_HGA1C_3MReviewed = value
            '    End Set
            'End Property


            Private _DiabetesScreening_HGA1C_3MNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_3MNAFor() As StringField
                Get
                    Return _DiabetesScreening_HGA1C_3MNAFor
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_HGA1C_3MNAFor = value
                End Set
            End Property

            Private _DiabetesScreening_HGA1C_3MPrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_3MPrescribed() As BooleanField
                Get
                    Return _DiabetesScreening_HGA1C_3MPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _DiabetesScreening_HGA1C_3MPrescribed = value
                End Set
            End Property


            Private _DiabetesScreening_HGA1C_6MDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_6MDate() As DateField
                Get
                    Return _DiabetesScreening_HGA1C_6MDate
                End Get
                Set(ByVal value As DateField)
                    _DiabetesScreening_HGA1C_6MDate = value
                End Set
            End Property

            Private _DiabetesScreening_HGA1C_6MResult As New StringField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_6MResult() As StringField
                Get
                    Return _DiabetesScreening_HGA1C_6MResult
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_HGA1C_6MResult = value
                End Set
            End Property

            'Private _DiabetesScreening_HGA1C_6MReviewed As New BooleanField
            '<DataMember()> _
            'Public Property DiabetesScreening_HGA1C_6MReviewed() As BooleanField
            '    Get
            '        Return _DiabetesScreening_HGA1C_6MReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _DiabetesScreening_HGA1C_6MReviewed = value
            '    End Set
            'End Property

            Private _DiabetesScreening_HGA1C_6MNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_6MNAFor() As StringField
                Get
                    Return _DiabetesScreening_HGA1C_6MNAFor
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_HGA1C_6MNAFor = value
                End Set
            End Property

            Private _DiabetesScreening_HGA1C_6MPrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_6MPrescribed() As BooleanField
                Get
                    Return _DiabetesScreening_HGA1C_6MPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _DiabetesScreening_HGA1C_6MPrescribed = value
                End Set
            End Property


            Private _DiabetesScreening_MicroalbuminDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_MicroalbuminDate() As DateField
                Get
                    Return _DiabetesScreening_MicroalbuminDate
                End Get
                Set(ByVal value As DateField)
                    _DiabetesScreening_MicroalbuminDate = value
                End Set
            End Property

            Private _DiabetesScreening_MicroalbuminResult As New StringField
            <DataMember()>
            Public Property DiabetesScreening_MicroalbuminResult() As StringField
                Get
                    Return _DiabetesScreening_MicroalbuminResult
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_MicroalbuminResult = value
                End Set
            End Property

            'Private _DiabetesScreening_MicroalbuminReviewed As New BooleanField
            '<DataMember()> _
            'Public Property DiabetesScreening_MicroalbuminReviewed() As BooleanField
            '    Get
            '        Return _DiabetesScreening_MicroalbuminReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _DiabetesScreening_MicroalbuminReviewed = value
            '    End Set
            'End Property

            Private _DiabetesScreening_MicroalbuminNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_MicroalbuminNAFor() As StringField
                Get
                    Return _DiabetesScreening_MicroalbuminNAFor
                End Get
                Set(ByVal value As StringField)
                    _DiabetesScreening_MicroalbuminNAFor = value
                End Set
            End Property

            Private _DiabetesScreening_MicroalbuminPrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_MicroalbuminPrescribed() As BooleanField
                Get
                    Return _DiabetesScreening_MicroalbuminPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _DiabetesScreening_MicroalbuminPrescribed = value
                End Set
            End Property


            Private _GlaucomaTestDate As New DateField
            <DataMember()>
            Public Property GlaucomaTestDate() As DateField
                Get
                    Return _GlaucomaTestDate
                End Get
                Set(ByVal value As DateField)
                    _GlaucomaTestDate = value
                End Set
            End Property

            Private _GlaucomaTestResult As New StringField
            <DataMember()>
            Public Property GlaucomaTestResult() As StringField
                Get
                    Return _GlaucomaTestResult
                End Get
                Set(ByVal value As StringField)
                    _GlaucomaTestResult = value
                End Set
            End Property

            'Private _GlaucomaTestReviewed As New BooleanField
            '<DataMember()> _
            'Public Property GlaucomaTestReviewed() As BooleanField
            '    Get
            '        Return _GlaucomaTestReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _GlaucomaTestReviewed = value
            '    End Set
            'End Property


            Private _GlaucomaTestNAFor As New StringField
            <DataMember()>
            Public Property GlaucomaTestNAFor() As StringField
                Get
                    Return _GlaucomaTestNAFor
                End Get
                Set(ByVal value As StringField)
                    _GlaucomaTestNAFor = value
                End Set
            End Property


            Private _GlaucomaTestPrescribed As New BooleanField
            <DataMember()>
            Public Property GlaucomaTestPrescribed() As BooleanField
                Get
                    Return _GlaucomaTestPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _GlaucomaTestPrescribed = value
                End Set
            End Property


            Private _Mammogram_ProstateCancerDate As New DateField
            <DataMember()>
            Public Property Mammogram_ProstateCancerDate() As DateField
                Get
                    Return _Mammogram_ProstateCancerDate
                End Get
                Set(ByVal value As DateField)
                    _Mammogram_ProstateCancerDate = value
                End Set
            End Property

            Private _Mammogram_ProstateCancerResult As New StringField
            <DataMember()>
            Public Property Mammogram_ProstateCancerResult() As StringField
                Get
                    Return _Mammogram_ProstateCancerResult
                End Get
                Set(ByVal value As StringField)
                    _Mammogram_ProstateCancerResult = value
                End Set
            End Property

            Private _Mammogram_CancerResult_NA As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_NA() As BooleanField
                Get
                    Return _Mammogram_CancerResult_NA
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_NA = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_0 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_0() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_0
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_0 = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_1 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_1() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_1
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_1 = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_2 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_2() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_2
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_2 = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_3 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_3() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_3
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_3 = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_4 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_4() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_4
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_4 = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_5 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_5() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_5
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_5 = value
                End Set
            End Property

            Private _Mammogram_CancerResult_Category_6 As New BooleanField
            <DataMember()>
            Public Property Mammogram_CancerResult_Category_6() As BooleanField
                Get
                    Return _Mammogram_CancerResult_Category_6
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_CancerResult_Category_6 = value
                End Set
            End Property

            'Private _Mammogram_ProstateCancerReviewed As New BooleanField
            '<DataMember()> _
            'Public Property Mammogram_ProstateCancerReviewed() As BooleanField
            '    Get
            '        Return _Mammogram_ProstateCancerReviewed
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _Mammogram_ProstateCancerReviewed = value
            '    End Set
            'End Property

            Private _Mammogram_ProstateCancerNAFor As New StringField
            <DataMember()>
            Public Property Mammogram_ProstateCancerNAFor() As StringField
                Get
                    Return _Mammogram_ProstateCancerNAFor
                End Get
                Set(ByVal value As StringField)
                    _Mammogram_ProstateCancerNAFor = value
                End Set
            End Property

            Private _Mammogram_ProstateCancerPrescribed As New BooleanField
            <DataMember()>
            Public Property Mammogram_ProstateCancerPrescribed() As BooleanField
                Get
                    Return _Mammogram_ProstateCancerPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _Mammogram_ProstateCancerPrescribed = value
                End Set
            End Property

            Private _MammogramCancerDate As New DateField
            <DataMember()>
            Public Property MammogramCancerDate() As DateField
                Get
                    Return _MammogramCancerDate
                End Get
                Set(ByVal value As DateField)
                    _MammogramCancerDate = value
                End Set
            End Property

            Private _MammogramCancerResult As New StringField
            <DataMember()>
            Public Property MammogramCancerResult() As StringField
                Get
                    Return _MammogramCancerResult
                End Get
                Set(ByVal value As StringField)
                    _MammogramCancerResult = value
                End Set
            End Property

            Private _MammogramCancerNAFor As New StringField
            <DataMember()>
            Public Property MammogramCancerNAFor() As StringField
                Get
                    Return _MammogramCancerNAFor
                End Get
                Set(ByVal value As StringField)
                    _MammogramCancerNAFor = value
                End Set
            End Property

            Private _MammogramCancerPrescribed As New BooleanField
            <DataMember()>
            Public Property MammogramCancerPrescribed() As BooleanField
                Get
                    Return _MammogramCancerPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _MammogramCancerPrescribed = value
                End Set
            End Property

            Private _ProstateCancerDate As New DateField
            <DataMember()>
            Public Property ProstateCancerDate() As DateField
                Get
                    Return _ProstateCancerDate
                End Get
                Set(ByVal value As DateField)
                    _ProstateCancerDate = value
                End Set
            End Property

            Private _ProstateCancerResult As New StringField
            <DataMember()>
            Public Property ProstateCancerResult() As StringField
                Get
                    Return _ProstateCancerResult
                End Get
                Set(ByVal value As StringField)
                    _ProstateCancerResult = value
                End Set
            End Property

            Private _ProstateCancerNAFor As New StringField
            <DataMember()>
            Public Property ProstateCancerNAFor() As StringField
                Get
                    Return _ProstateCancerNAFor
                End Get
                Set(ByVal value As StringField)
                    _ProstateCancerNAFor = value
                End Set
            End Property

            Private _ProstateCancerPrescribed As New BooleanField
            <DataMember()>
            Public Property ProstateCancerPrescribed() As BooleanField
                Get
                    Return _ProstateCancerPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _ProstateCancerPrescribed = value
                End Set
            End Property

            Private _FluShotPrescribed As New BooleanField
            <DataMember()>
            Public Property FluShotPrescribed() As BooleanField
                Get
                    Return _FluShotPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _FluShotPrescribed = value
                End Set
            End Property


            Private _FluShotDate As New DateField
            <DataMember()>
            Public Property FluShotDate() As DateField
                Get
                    Return _FluShotDate
                End Get
                Set(ByVal value As DateField)
                    _FluShotDate = value
                End Set
            End Property

            Private _FluShotComments As New StringField
            <DataMember()>
            Public Property FluShotComments() As StringField
                Get
                    Return _FluShotComments
                End Get
                Set(ByVal value As StringField)
                    _FluShotComments = value
                End Set
            End Property

            Private _FluShot_PatientRefuses As New BooleanField
            <DataMember()>
            Public Property FluShot_PatientRefuses() As BooleanField
                Get
                    Return _FluShot_PatientRefuses
                End Get
                Set(ByVal value As BooleanField)
                    _FluShot_PatientRefuses = value
                End Set
            End Property


            Private _PneumococcalShotComments As New StringField
            <DataMember()>
            Public Property PneumococcalShotComments() As StringField
                Get
                    Return _PneumococcalShotComments
                End Get
                Set(ByVal value As StringField)
                    _PneumococcalShotComments = value
                End Set
            End Property

            Private _PneumococcalShotDate As New DateField
            <DataMember()>
            Public Property PneumococcalShotDate() As DateField
                Get
                    Return _PneumococcalShotDate
                End Get
                Set(ByVal value As DateField)
                    _PneumococcalShotDate = value
                End Set
            End Property

            Private _PneumococcalShotPrescribed As New BooleanField
            <DataMember()>
            Public Property PneumococcalShotPrescribed() As BooleanField
                Get
                    Return _PneumococcalShotPrescribed
                End Get
                Set(ByVal value As BooleanField)
                    _PneumococcalShotPrescribed = value
                End Set
            End Property

            Private _PneumococcalShot_PatientRefuses As New BooleanField
            <DataMember()>
            Public Property PneumococcalShot_PatientRefuses() As BooleanField
                Get
                    Return _PneumococcalShot_PatientRefuses
                End Get
                Set(ByVal value As BooleanField)
                    _PneumococcalShot_PatientRefuses = value
                End Set
            End Property

            <DataMember()>
            Public Property ColorectalFITDNA() As New BooleanField
            <DataMember()>
            Public Property ColorectalFITDNADate() As New DateField
            <DataMember()>
            Public Property ColorectalFITDNAResult() As New StringField
            <DataMember()>
            Public Property ColorectalFITDNANAFor() As New StringField
            <DataMember()>
            Public Property ColorectalFITDNAPrescribe() As New BooleanField

            <DataMember()>
            Public Property ColorectalColonographyCT() As New BooleanField
            <DataMember()>
            Public Property ColorectalColonographyCTDate() As New DateField
            <DataMember()>
            Public Property ColorectalColonographyCTResult() As New StringField
            <DataMember()>
            Public Property ColorectalColonographyCTNAFor() As New StringField
            <DataMember()>
            Public Property ColorectalColonographyCTPrescribe() As New BooleanField

            <DataMember()>
            Public Property DiabetesScreening_HGA1C_Date() As New DateField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_Result() As New StringField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_NAFor() As New StringField
            <DataMember()>
            Public Property DiabetesScreening_HGA1C_Prescribed() As New BooleanField

            <DataMember()>
            Public Property CASecIsRequired As Boolean

            <DataMember()>
            Public Property PAPSMEAR_Date As New DateField
            <DataMember()>
            Public Property PAPSMEAR_Result As New StringField
            <DataMember()>
            Public Property PAPSMEAR_NAFor As New StringField
            <DataMember()>
            Public Property PAPSMEAR_Prescribed As New BooleanField
            <DataMember()>
            Public Property Screening_HPV_Ordered As New BooleanField
            <DataMember()>
            Public Property Screening_HPV_Result As New StringField
            <DataMember()>
            Public Property Screening_HPV_Date As New DateField
            <DataMember()>
            Public Property Screening_HPV_Comment As New StringField
            <DataMember()>
            Public Property Retinopathy As New BooleanField
            <DataMember()>
            Public Property Proliferative As New BooleanField
            <DataMember()>
            Public Property ProliferativeEyeRT As New BooleanField
            <DataMember()>
            Public Property ProliferativeEyeLT As New BooleanField
            <DataMember()>
            Public Property TdTdap_Comments As New StringField
            <DataMember()>
            Public Property TdTdap_Done_Date As New DateField
            <DataMember()>
            Public Property TdTdap_Prescribed As New BooleanField
            <DataMember()>
            Public Property TdTdap_PatientRefuses As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_Urine_AlbuminDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_Urine_AlbuminResult As New DecimalField
            <DataMember()>
            Public Property DiabetesScreening_Urine_AlbuminNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_Urine_AlbuminPrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_Urine_CreatinineDate As New DateField
            <DataMember()>
            Public Property DiabetesScreening_Urine_CreatinineResult As New DecimalField
            <DataMember()>
            Public Property DiabetesScreening_Urine_CreatinineNAFor As New StringField
            <DataMember()>
            Public Property DiabetesScreening_Urine_CreatininePrescribed As New BooleanField
            <DataMember()>
            Public Property DiabetesScreening_CreatinineAlbumine_Ratio As New DecimalField
            <DataMember()>
            Public Property Retinopathy_Negative As New BooleanField
            <DataMember()>
            Public Property Retinopathy_Negative_Eye As New IntegerField
            <DataMember()>
            Public Property Screening_Retinopathy_Eye As New IntegerField
            <DataMember()>
            Public Property Screening_Proliferative_Eye As New IntegerField
            <DataMember()>
            Public Property Screening_Eye_Severity As BooleanField
            <DataMember()>
            Public Property Screening_Eye_SeverityLevel As IntegerField
            <DataMember()>
            Public Property MacularEdema As New BooleanField
            <DataMember()>
            Public Property MacularEdema_Eye As New IntegerField
            <DataMember()>
            Public Property ScreeningRetinopathy_NA As New BooleanField

        End Class

#End Region

#Region " Assessment & Plan of Treatment"

        <Serializable(), DataContract()>
        Public Class AssessmentPlanOfTreatmentSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "APT"
            End Sub

            Private _No As New BooleanField
            <DataMember()>
            Public Property No() As BooleanField
                Get
                    Return _No
                End Get
                Set(ByVal value As BooleanField)
                    _No = value
                End Set
            End Property

            Private _DMType As New BooleanField 'TRUE for DM Type 1, FALSE for DM Type 2, NULL not DM Type selected.
            <DataMember()>
            Public Property DMType() As BooleanField
                Get
                    Return _DMType
                End Get
                Set(ByVal value As BooleanField)
                    _DMType = value
                End Set
            End Property

            Private _DMSecondary As New BooleanField
            <DataMember()>
            Public Property DMSecondary() As BooleanField
                Get
                    Return _DMSecondary
                End Get
                Set(ByVal value As BooleanField)
                    _DMSecondary = value
                End Set
            End Property

            Private _Controlled As New BooleanField 'TRUE for Controlled, FALSE for UnControlled, NULL not Controlled or UnControlled selected.
            <DataMember()>
            Public Property Controlled() As BooleanField
                Get
                    Return _Controlled
                End Get
                Set(ByVal value As BooleanField)
                    _Controlled = value
                End Set
            End Property

            Private _PoorlyController As New BooleanField
            <DataMember()>
            Public Property PoorlyController() As BooleanField
                Get
                    Return _PoorlyController
                End Get
                Set(ByVal value As BooleanField)
                    _PoorlyController = value
                End Set
            End Property

            Private _DMComments As New StringField
            <DataMember()>
            Public Property DMComments() As StringField
                Get
                    Return _DMComments
                End Get
                Set(ByVal value As StringField)
                    _DMComments = value
                End Set
            End Property

            Private _DiabeticNeuropathy As New BooleanField
            <DataMember()>
            Public Property DiabeticNeuropathy() As BooleanField
                Get
                    Return _DiabeticNeuropathy
                End Get
                Set(ByVal value As BooleanField)
                    _DiabeticNeuropathy = value
                End Set
            End Property

            Private _DiabeticNeuropathyComments As New StringField
            <DataMember()>
            Public Property DiabeticNeuropathyComments() As StringField
                Get
                    Return _DiabeticNeuropathyComments
                End Get
                Set(ByVal value As StringField)
                    _DiabeticNeuropathyComments = value
                End Set
            End Property

            Private _DiabeticPVD As New BooleanField
            <DataMember()>
            Public Property DiabeticPVD() As BooleanField
                Get
                    Return _DiabeticPVD
                End Get
                Set(ByVal value As BooleanField)
                    _DiabeticPVD = value
                End Set
            End Property

            Private _DiabeticPVDComments As New StringField
            <DataMember()>
            Public Property DiabeticPVDComments() As StringField
                Get
                    Return _DiabeticPVDComments
                End Get
                Set(ByVal value As StringField)
                    _DiabeticPVDComments = value
                End Set
            End Property

            Private _DiabeticNephropathy As New BooleanField
            <DataMember()>
            Public Property DiabeticNephropathy() As BooleanField
                Get
                    Return _DiabeticNephropathy
                End Get
                Set(ByVal value As BooleanField)
                    _DiabeticNephropathy = value
                End Set
            End Property

            Private _DiabeticNephropathyComments As New StringField
            <DataMember()>
            Public Property DiabeticNephropathyComments() As StringField
                Get
                    Return _DiabeticNephropathyComments
                End Get
                Set(ByVal value As StringField)
                    _DiabeticNephropathyComments = value
                End Set
            End Property

            'Private _DiabeticCKDStage As New IntegerField
            '<DataMember()> _
            'Public Property DiabeticCKDStage() As IntegerField
            '    Get
            '        Return _DiabeticCKDStage
            '    End Get
            '    Set(ByVal value As IntegerField)
            '        _DiabeticCKDStage = value
            '    End Set
            'End Property

            'Private _DMWithOphtalmicManifestations As New StringField
            '<DataMember()> _
            'Public Property DMWithOphtalmicManifestations() As StringField
            '    Get
            '        Return _DMWithOphtalmicManifestations
            '    End Get
            '    Set(ByVal value As StringField)
            '        _DMWithOphtalmicManifestations = value
            '    End Set
            'End Property

            Private _OtherDiabeticComplication As New StringField
            <DataMember()>
            Public Property OtherDiabeticComplication() As StringField
                Get
                    Return _OtherDiabeticComplication
                End Get
                Set(ByVal value As StringField)
                    _OtherDiabeticComplication = value
                End Set
            End Property

            Private _OtherDiabeticComplicationComments As New StringField
            <DataMember()>
            Public Property OtherDiabeticComplicationComments() As StringField
                Get
                    Return _OtherDiabeticComplicationComments
                End Get
                Set(ByVal value As StringField)
                    _OtherDiabeticComplicationComments = value
                End Set
            End Property

            'Private _PlanOfTreatment As New StringField
            '<DataMember()> _
            'Public Property PlanOfTreatment() As StringField
            '    Get
            '        Return _PlanOfTreatment
            '    End Get
            '    Set(ByVal value As StringField)
            '        _PlanOfTreatment = value
            '    End Set
            'End Property

            'Private _InsulinUsage As New BooleanField
            '<DataMember()> _
            'Public Property InsulinUsage() As BooleanField
            '    Get
            '        Return _InsulinUsage
            '    End Get
            '    Set(ByVal value As BooleanField)
            '        _InsulinUsage = value
            '    End Set
            'End Property

            Private _DiabeticCataracts As New BooleanField
            <DataMember()>
            Public Property DiabeticCataracts() As BooleanField
                Get
                    Return _DiabeticCataracts
                End Get
                Set(ByVal value As BooleanField)
                    _DiabeticCataracts = value
                End Set
            End Property

            Private _DiabeticCataractsComments As New StringField
            <DataMember()>
            Public Property DiabeticCataractsComments() As StringField
                Get
                    Return _DiabeticCataractsComments
                End Get
                Set(ByVal value As StringField)
                    _DiabeticCataractsComments = value
                End Set
            End Property

            Private _Retinopathy As New BooleanField
            <DataMember()>
            Public Property Retinopathy() As BooleanField
                Get
                    Return _Retinopathy
                End Get
                Set(ByVal value As BooleanField)
                    _Retinopathy = value
                End Set
            End Property

            Private _RetinopathyComments As New StringField
            <DataMember()>
            Public Property RetinopathyComments() As StringField
                Get
                    Return _RetinopathyComments
                End Get
                Set(ByVal value As StringField)
                    _RetinopathyComments = value
                End Set
            End Property

            Private _Proliferative As New BooleanField
            <DataMember()>
            Public Property Proliferative() As BooleanField
                Get
                    Return _Proliferative
                End Get
                Set(ByVal value As BooleanField)
                    _Proliferative = value
                End Set
            End Property

            Private _ProliferativeComments As New StringField
            <DataMember()>
            Public Property ProliferativeComments() As StringField
                Get
                    Return _ProliferativeComments
                End Get
                Set(ByVal value As StringField)
                    _ProliferativeComments = value
                End Set
            End Property

            Private _Dermatitis As New BooleanField
            <DataMember()>
            Public Property Dermatitis() As BooleanField
                Get
                    Return _Dermatitis
                End Get
                Set(ByVal value As BooleanField)
                    _Dermatitis = value
                End Set
            End Property

            Private _DermatitisComments As New StringField
            <DataMember()>
            Public Property DermatitisComments() As StringField
                Get
                    Return _DermatitisComments
                End Get
                Set(ByVal value As StringField)
                    _DermatitisComments = value
                End Set
            End Property

            Private _Periodontal As New BooleanField
            <DataMember()>
            Public Property Periodontal() As BooleanField
                Get
                    Return _Periodontal
                End Get
                Set(ByVal value As BooleanField)
                    _Periodontal = value
                End Set
            End Property

            Private _PeriodontalComments As New StringField
            <DataMember()>
            Public Property PeriodontalComments() As StringField
                Get
                    Return _PeriodontalComments
                End Get
                Set(ByVal value As StringField)
                    _PeriodontalComments = value
                End Set
            End Property

            <DataMember()>
            Public Property DMSecondaryText As New StringField
            <DataMember()>
            Public Property OutOfControl As New BooleanField
            <DataMember()>
            Public Property UncontrolledWithHyperglycemia As New BooleanField
            <DataMember()>
            Public Property UncontrolledWithHypoglycemia As New BooleanField
            <DataMember()>
            Public Property HyperlipidemiaDueDM As New BooleanField
            <DataMember()>
            Public Property DMPlanAndTreatmentComments1 As New StringField
            <DataMember()>
            Public Property DMPlanAndTreatmentComments2 As New StringField
            <DataMember()>
            Public Property DMPlanAndTreatmentComments3 As New StringField
            <DataMember()>
            Public Property DiabeticArthropathy As New BooleanField
            <DataMember()>
            Public Property DiabeticArthropathyComment As New StringField
            <DataMember()>
            Public Property GestionalDiabetes As New BooleanField
            <DataMember()>
            Public Property DermatitisTypeLocation As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)

            <DataMember()>
            Public Property GestionalDiabetesComment As New StringField

            'Add 2026 
            Private _Remission As New BooleanField
            <DataMember()>
            Public Property Remission() As BooleanField
                Get
                    Return _Remission
                End Get
                Set(ByVal value As BooleanField)
                    _Remission = value
                End Set
            End Property

        End Class

#End Region

#Region " Only for Cancer Diagnosis"

        <Serializable(), DataContract()>
        Public Class CancerDiagnosisSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "CD"
                Me.History() = New BooleanField()
                Me.Primary() = New BooleanField()
                Me.Secondary() = New BooleanField()
                Me.Remission() = New BooleanField()
                Me.Active() = New BooleanField()
                Me.Diagnoses() = New StringField()
                Me.Treatment() = New StringField()
                Me.CurrentlyInChemotherapy = New BooleanField()
                Me.CurrentlyInRadiotherapy = New BooleanField()
                Me.CurrentlyInImmunotherapy = New BooleanField()
            End Sub

            Private _History As New BooleanField
            <DataMember()>
            Public Property History() As BooleanField
                Get
                    Return _History
                End Get
                Set(ByVal value As BooleanField)
                    _History = value
                End Set
            End Property

            Private _Primary As New BooleanField
            <DataMember()>
            Public Property Primary() As BooleanField
                Get
                    Return _Primary
                End Get
                Set(ByVal value As BooleanField)
                    _Primary = value
                End Set
            End Property

            Private _Secondary As New BooleanField
            <DataMember()>
            Public Property Secondary() As BooleanField
                Get
                    Return _Secondary
                End Get
                Set(ByVal value As BooleanField)
                    _Secondary = value
                End Set
            End Property

            Private _Remission As New BooleanField
            <DataMember()>
            Public Property Remission() As BooleanField
                Get
                    Return _Remission
                End Get
                Set(ByVal value As BooleanField)
                    _Remission = value
                End Set
            End Property

            Private _Active As New BooleanField
            <DataMember()>
            Public Property Active() As BooleanField
                Get
                    Return _Active
                End Get
                Set(ByVal value As BooleanField)
                    _Active = value
                End Set
            End Property

            Private _Diagnoses As New StringField
            <DataMember()>
            Public Property Diagnoses() As StringField
                Get
                    Return _Diagnoses
                End Get
                Set(ByVal value As StringField)
                    _Diagnoses = value
                End Set
            End Property

            Private _Treatment As New StringField
            <DataMember()>
            Public Property Treatment() As StringField
                Get
                    Return _Treatment
                End Get
                Set(ByVal value As StringField)
                    _Treatment = value
                End Set
            End Property

            <DataMember()>
            Public Property CurrentlyInChemotherapy As New BooleanField
            <DataMember()>
            Public Property CurrentlyInRadiotherapy As New BooleanField
            <DataMember()>
            Public Property CurrentlyInImmunotherapy As New BooleanField
            <DataMember()>
            Public Property CurrentlyRefusesTreatment As New BooleanField

        End Class

#End Region

#Region " Other Current Diagnosis"

        <Serializable(), DataContract()>
        Public Class OtherCurrentConditionsSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "OCC"
            End Sub

            Private _Controlled As New BooleanField 'TRUE for Controlled, FALSE for UnControlled, NULL not Controlled or UnControlled selected.
            <DataMember()>
            Public Property Controlled() As BooleanField
                Get
                    Return _Controlled
                End Get
                Set(ByVal value As BooleanField)
                    _Controlled = value
                End Set
            End Property

            Private _Diagnoses As New StringField
            <DataMember()>
            Public Property Diagnoses() As StringField
                Get
                    Return _Diagnoses
                End Get
                Set(ByVal value As StringField)
                    _Diagnoses = value
                End Set
            End Property

            Private _Treatment As New StringField
            <DataMember()>
            Public Property Treatment() As StringField
                Get
                    Return _Treatment
                End Get
                Set(ByVal value As StringField)
                    _Treatment = value
                End Set
            End Property

            Private _DiagnosesCode As New StringField
            <DataMember()>
            Public Property DiagnosesCode() As StringField
                Get
                    Return _DiagnosesCode
                End Get
                Set(ByVal value As StringField)
                    _DiagnosesCode = value
                End Set
            End Property

            'Private _IsInternal As New BooleanField
            '<DataMember()>
            'Public Property IsInternal() As BooleanField
            '    Get
            '        Return _IsInternal
            '    End Get
            '    Set(value As BooleanField)
            '        _IsInternal = value
            '    End Set
            'End Property

        End Class

        <Serializable(), DataContract()>
        Public Class OtherCurrentConditionsAdditionalSection
            Inherits AHASectionErrorSummary

            Private _AdditionalRecomendation As New StringField
            <DataMember()>
            Public Property AdditionalRecomendation() As StringField
                Get
                    Return _AdditionalRecomendation
                End Get
                Set(ByVal value As StringField)
                    _AdditionalRecomendation = value
                End Set
            End Property

        End Class

#End Region

#Region " Enfermedad Cronica del Riñon (CKD)"

        <Serializable(), DataContract()>
        Public Class ChronicKidneyDiseaseSection
            Inherits AHASectionErrorSummary

            Private _NA As New BooleanField

            <DataMember()>
            Public Property NA() As BooleanField
                Get
                    Return _NA
                End Get
                Set(ByVal value As BooleanField)
                    _NA = value
                End Set
            End Property

            Private _Stage As New IntegerField ', NULL = no selected stage option, 1 - 4 (and -3 for 3b stage 2021 change) = Stage Value
            <DataMember()>
            Public Property Stage() As IntegerField
                Get
                    Return _Stage
                End Get
                Set(ByVal value As IntegerField)
                    _Stage = value
                End Set
            End Property

            Private _DueToDM As New BooleanField
            <DataMember()>
            Public Property DueToDM() As BooleanField
                Get
                    Return _DueToDM
                End Get
                Set(ByVal value As BooleanField)
                    _DueToDM = value
                End Set
            End Property

            Private _DueToOtherCondition As New StringField
            <DataMember()>
            Public Property DueToOtherCondition() As StringField
                Get
                    Return _DueToOtherCondition
                End Get
                Set(ByVal value As StringField)
                    _DueToOtherCondition = value
                End Set
            End Property

            Private _Controlled As New BooleanField
            <DataMember()>
            Public Property Controlled() As BooleanField
                Get
                    Return _Controlled
                End Get
                Set(ByVal value As BooleanField)
                    _Controlled = value
                End Set
            End Property

            Private _LowFatDiet As New BooleanField
            <DataMember()>
            Public Property LowFatDiet() As BooleanField
                Get
                    Return _LowFatDiet
                End Get
                Set(ByVal value As BooleanField)
                    _LowFatDiet = value
                End Set
            End Property

            Private _Dialysis As New BooleanField
            <DataMember()>
            Public Property Dialysis() As BooleanField
                Get
                    Return _Dialysis
                End Get
                Set(ByVal value As BooleanField)
                    _Dialysis = value
                End Set
            End Property

            Private _NoMeetDialysis As New BooleanField
            <DataMember()>
            Public Property NoMeetDialysis() As BooleanField
                Get
                    Return _NoMeetDialysis
                End Get
                Set(ByVal value As BooleanField)
                    _NoMeetDialysis = value
                End Set
            End Property

            Private _AdditionalTreatment As New StringField
            <DataMember()>
            Public Property AdditionalTreatment() As StringField
                Get
                    Return _AdditionalTreatment
                End Get
                Set(ByVal value As StringField)
                    _AdditionalTreatment = value
                End Set
            End Property

            Private _Hyperparathyroidism As New BooleanField
            <DataMember()>
            Public Property Hyperparathyroidism() As BooleanField
                Get
                    Return _Hyperparathyroidism
                End Get
                Set(ByVal value As BooleanField)
                    _Hyperparathyroidism = value
                End Set
            End Property

            Private _HyperparathyroidismTreatment As New StringField
            <DataMember()>
            Public Property HyperparathyroidismTreatment() As StringField
                Get
                    Return _HyperparathyroidismTreatment
                End Get
                Set(ByVal value As StringField)
                    _HyperparathyroidismTreatment = value
                End Set
            End Property

            Private _GFR As String
            <DataMember()>
            Public Property GFR() As String
                Get
                    Return _GFR
                End Get
                Set(ByVal value As String)
                    _GFR = value
                End Set
            End Property

            Private _SerumCalcium As String
            <DataMember()>
            Public Property SerumCalcium() As String
                Get
                    Return _SerumCalcium
                End Get
                Set(ByVal value As String)
                    _SerumCalcium = value
                End Set
            End Property

            Private _SerumPTH As String
            <DataMember()>
            Public Property SerumPTH() As String
                Get
                    Return _SerumPTH
                End Get
                Set(ByVal value As String)
                    _SerumPTH = value
                End Set
            End Property

            Private _StressIncontinence As New BooleanField
            <DataMember()>
            Public Property StressIncontinence() As BooleanField
                Get
                    Return _StressIncontinence
                End Get
                Set(ByVal value As BooleanField)
                    _StressIncontinence = value
                End Set
            End Property

            Private _UrgeIncontinence As New BooleanField
            <DataMember()>
            Public Property UrgeIncontinence() As BooleanField
                Get
                    Return _UrgeIncontinence
                End Get
                Set(ByVal value As BooleanField)
                    _UrgeIncontinence = value
                End Set
            End Property

            Private _PostMicturitionDribble As New BooleanField
            <DataMember()>
            Public Property PostMicturitionDribble() As BooleanField
                Get
                    Return _PostMicturitionDribble
                End Get
                Set(ByVal value As BooleanField)
                    _PostMicturitionDribble = value
                End Set
            End Property

            Private _OveractiveBladder As New BooleanField
            <DataMember()>
            Public Property OveractiveBladder() As BooleanField
                Get
                    Return _OveractiveBladder
                End Get
                Set(ByVal value As BooleanField)
                    _OveractiveBladder = value
                End Set
            End Property

            Private _BladderTreatmentPlan As New StringField
            <DataMember()>
            Public Property BladderTreatmentPlan() As StringField
                Get
                    Return _BladderTreatmentPlan
                End Get
                Set(ByVal value As StringField)
                    _BladderTreatmentPlan = value
                End Set
            End Property

            Private _KidneyTransplant As New BooleanField
            <DataMember()>
            Public Property KidneyTransplant() As BooleanField
                Get
                    Return _KidneyTransplant
                End Get
                Set(ByVal value As BooleanField)
                    _KidneyTransplant = value
                End Set
            End Property

            Private _GFROrdered As New BooleanField
            <DataMember()>
            Public Property GFROrdered() As BooleanField
                Get
                    Return _GFROrdered
                End Get
                Set(ByVal value As BooleanField)
                    _GFROrdered = value
                End Set
            End Property

            Private _GFRDate As New DateField
            <DataMember()>
            Public Property GFRDate() As DateField
                Get
                    Return _GFRDate
                End Get
                Set(ByVal value As DateField)
                    _GFRDate = value
                End Set
            End Property

            <DataMember()>
            Public Property Nephropathy As New BooleanField
            <DataMember()>
            Public Property NephropathyType As New StringField
            <DataMember()>
            Public Property Nephritis As New BooleanField
            <DataMember()>
            Public Property NephritisType As New StringField
            <DataMember>
            Public Property HasFistula As New BooleanField
            <DataMember>
            Public Property CKDBox As New BooleanField
            <DataMember()>
            Public Property MedicationList As New List(Of String)


        End Class

#End Region

#Region " Ulceras por Presion"

        <Serializable(), DataContract()>
        Public Class PressureSoresSection
            Inherits AHASectionErrorSummary

            Private _NA As New BooleanField
            <DataMember()>
            Public Property NA() As BooleanField
                Get
                    Return _NA
                End Get
                Set(ByVal value As BooleanField)
                    _NA = value
                End Set
            End Property

            Private _HighBackPressureUlcerStage As New IntegerField
            <DataMember()>
            Public Property HighBackPressureUlcerStage() As IntegerField
                Get
                    Return _HighBackPressureUlcerStage
                End Get
                Set(ByVal value As IntegerField)
                    _HighBackPressureUlcerStage = value
                End Set
            End Property

            Private _LowBackPressureUlcerStage As New IntegerField
            <DataMember()>
            Public Property LowBackPressureUlcerStage() As IntegerField
                Get
                    Return _LowBackPressureUlcerStage
                End Get
                Set(ByVal value As IntegerField)
                    _LowBackPressureUlcerStage = value
                End Set
            End Property

            Private _HipPressureUlcerStageLeft As New IntegerField
            <DataMember()>
            Public Property HipPressureUlcerStageLeft() As IntegerField
                Get
                    Return _HipPressureUlcerStageLeft
                End Get
                Set(ByVal value As IntegerField)
                    _HipPressureUlcerStageLeft = value
                End Set
            End Property

            Private _HipPressureUlcerStageRight As New IntegerField
            <DataMember()>
            Public Property HipPressureUlcerStageRight() As IntegerField
                Get
                    Return _HipPressureUlcerStageRight
                End Get
                Set(ByVal value As IntegerField)
                    _HipPressureUlcerStageRight = value
                End Set
            End Property


            Private _HipPressureUlcerLeft As New BooleanField
            <DataMember()>
            Public Property HipPressureUlcerLeft() As BooleanField
                Get
                    Return _HipPressureUlcerLeft
                End Get
                Set(ByVal value As BooleanField)
                    _HipPressureUlcerLeft = value
                End Set
            End Property

            Private _HipPressureUlcerRight As New BooleanField
            <DataMember()>
            Public Property HipPressureUlcerRight() As BooleanField
                Get
                    Return _HipPressureUlcerRight
                End Get
                Set(ByVal value As BooleanField)
                    _HipPressureUlcerRight = value
                End Set
            End Property

            Private _HeelPressureUlcerStageLeft As New IntegerField
            <DataMember()>
            Public Property HeelPressureUlcerStageLeft() As IntegerField
                Get
                    Return _HeelPressureUlcerStageLeft
                End Get
                Set(ByVal value As IntegerField)
                    _HeelPressureUlcerStageLeft = value
                End Set
            End Property

            Private _HeelPressureUlcerStageRight As New IntegerField
            <DataMember()>
            Public Property HeelPressureUlcerStageRight() As IntegerField
                Get
                    Return _HeelPressureUlcerStageRight
                End Get
                Set(ByVal value As IntegerField)
                    _HeelPressureUlcerStageRight = value
                End Set
            End Property

            Private _HeelPressureUlcerLeft As New BooleanField
            <DataMember()>
            Public Property HeelPressureUlcerLeft() As BooleanField
                Get
                    Return _HeelPressureUlcerLeft
                End Get
                Set(ByVal value As BooleanField)
                    _HeelPressureUlcerLeft = value
                End Set
            End Property

            Private _HeelPressureUlcerRight As New BooleanField
            <DataMember()>
            Public Property HeelPressureUlcerRight() As BooleanField
                Get
                    Return _HeelPressureUlcerRight
                End Get
                Set(ByVal value As BooleanField)
                    _HeelPressureUlcerRight = value
                End Set
            End Property

            Private _OtherAreasStage As New IntegerField
            <DataMember()>
            Public Property OtherAreasStage() As IntegerField
                Get
                    Return _OtherAreasStage
                End Get
                Set(ByVal value As IntegerField)
                    _OtherAreasStage = value
                End Set
            End Property

            Private _OtherAreas As New StringField
            <DataMember()>
            Public Property OtherAreas() As StringField
                Get
                    Return _OtherAreas
                End Get
                Set(ByVal value As StringField)
                    _OtherAreas = value
                End Set
            End Property

            Private _Healing As New BooleanField
            <DataMember()>
            Public Property Healing() As BooleanField
                Get
                    Return _Healing
                End Get
                Set(ByVal value As BooleanField)
                    _Healing = value
                End Set
            End Property

            Private _Healed As New BooleanField
            <DataMember()>
            Public Property Healed() As BooleanField
                Get
                    Return _Healed
                End Get
                Set(ByVal value As BooleanField)
                    _Healed = value
                End Set
            End Property

            Private _Worse As New BooleanField
            <DataMember()>
            Public Property Worse() As BooleanField
                Get
                    Return _Worse
                End Get
                Set(ByVal value As BooleanField)
                    _Worse = value
                End Set
            End Property

            Private _Hydrocolloid As New BooleanField
            <DataMember()>
            Public Property Hydrocolloid() As BooleanField
                Get
                    Return _Hydrocolloid
                End Get
                Set(ByVal value As BooleanField)
                    _Hydrocolloid = value
                End Set
            End Property

            Private _SilverDressing As New BooleanField
            <DataMember()>
            Public Property SilverDressing() As BooleanField
                Get
                    Return _SilverDressing
                End Get
                Set(ByVal value As BooleanField)
                    _SilverDressing = value
                End Set
            End Property

            Private _Hydrogel As New BooleanField
            <DataMember()>
            Public Property Hydrogel() As BooleanField
                Get
                    Return _Hydrogel
                End Get
                Set(ByVal value As BooleanField)
                    _Hydrogel = value
                End Set
            End Property

            Private _Antibiotic As New BooleanField
            <DataMember()>
            Public Property Antibiotic() As BooleanField
                Get
                    Return _Antibiotic
                End Get
                Set(ByVal value As BooleanField)
                    _Antibiotic = value
                End Set
            End Property

            Private _Alginate As New BooleanField
            <DataMember()>
            Public Property Alginate() As BooleanField
                Get
                    Return _Alginate
                End Get
                Set(ByVal value As BooleanField)
                    _Alginate = value
                End Set
            End Property

            Private _Enzyme As New BooleanField
            <DataMember()>
            Public Property Enzyme() As BooleanField
                Get
                    Return _Enzyme
                End Get
                Set(ByVal value As BooleanField)
                    _Enzyme = value
                End Set
            End Property

            Private _TransparentDressing As New BooleanField
            <DataMember()>
            Public Property TransparentDressing() As BooleanField
                Get
                    Return _TransparentDressing
                End Get
                Set(ByVal value As BooleanField)
                    _TransparentDressing = value
                End Set
            End Property

            Private _OthersTreatment As New StringField
            <DataMember()>
            Public Property OthersTreatment() As StringField
                Get
                    Return _OthersTreatment
                End Get
                Set(ByVal value As StringField)
                    _OthersTreatment = value
                End Set
            End Property

            Private _ByPressure As New BooleanField
            <DataMember()>
            Public Property ByPressure() As BooleanField
                Get
                    Return _ByPressure
                End Get
                Set(ByVal value As BooleanField)
                    _ByPressure = value
                End Set
            End Property

            Private _Chronicle As New BooleanField
            <DataMember()>
            Public Property Chronicle() As BooleanField
                Get
                    Return _Chronicle
                End Get
                Set(ByVal value As BooleanField)
                    _Chronicle = value
                End Set
            End Property

            Private _AnatomicalSite As New StringField
            <DataMember()>
            Public Property AnatomicalSite() As StringField
                Get
                    Return _AnatomicalSite
                End Get
                Set(ByVal value As StringField)
                    _AnatomicalSite = value
                End Set
            End Property

            Private _ByVaricoseVeinsInLegs As BooleanField
            <DataMember()>
            Public Property ByVaricoseVainsInLegs() As BooleanField
                Get
                    Return _ByVaricoseVeinsInLegs
                End Get
                Set(ByVal value As BooleanField)
                    _ByVaricoseVeinsInLegs = value
                End Set
            End Property

            Private _ByArteriosclerosisInExtremities As BooleanField
            <DataMember()>
            Public Property ByArteriosclerosisInExtremities() As BooleanField
                Get
                    Return _ByArteriosclerosisInExtremities
                End Get
                Set(ByVal value As BooleanField)
                    _ByArteriosclerosisInExtremities = value
                End Set
            End Property

            Private _ByDiabetic As BooleanField
            <DataMember()>
            Public Property ByDiabetic() As BooleanField
                Get
                    Return _ByDiabetic
                End Get
                Set(ByVal value As BooleanField)
                    _ByDiabetic = value
                End Set
            End Property

            Private _ByPressureStage As IntegerField
            <DataMember()>
            Public Property ByPressureStage() As IntegerField
                Get
                    Return _ByPressureStage
                End Get
                Set(ByVal value As IntegerField)
                    _ByPressureStage = value
                End Set
            End Property

            Private _AnatomicalSiteOther As StringField
            <DataMember()>
            Public Property AnatomicalSiteOther() As StringField
                Get
                    Return _AnatomicalSiteOther
                End Get
                Set(ByVal value As StringField)
                    _AnatomicalSiteOther = value
                End Set
            End Property

            Private _ByOtherCondition As BooleanField
            <DataMember()>
            Public Property ByOtherCondition() As BooleanField
                Get
                    Return _ByOtherCondition
                End Get
                Set(ByVal value As BooleanField)
                    _ByOtherCondition = value
                End Set
            End Property

            Private _OtherConditionText As StringField
            <DataMember()>
            Public Property OtherConditionText() As StringField
                Get
                    Return _OtherConditionText
                End Get
                Set(ByVal value As StringField)
                    _OtherConditionText = value
                End Set
            End Property

            Private _Treatment As StringField
            <DataMember()>
            Public Property Treatment() As StringField
                Get
                    Return _Treatment
                End Get
                Set(ByVal value As StringField)
                    _Treatment = value
                End Set
            End Property

            <DataMember()>
            Public Property MedicationList As New List(Of String)

        End Class

#End Region

#Region " Rheumatoid Arthritis Section"

        <Serializable(), DataContract()>
        Public Class RheumatoidArthritisSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "RA"
                Me.RAOtherManifestationsCheckBox = New BooleanField()
            End Sub

            Private _RANoManifestations As New BooleanField
            <DataMember()>
            Public Property RANoManifestations() As BooleanField
                Get
                    Return _RANoManifestations
                End Get
                Set(ByVal value As BooleanField)
                    _RANoManifestations = value
                End Set
            End Property

            Private _RAWithPolyneuropathy As New BooleanField
            <DataMember()>
            Public Property RAWithPolyneuropathy() As BooleanField
                Get
                    Return _RAWithPolyneuropathy
                End Get
                Set(ByVal value As BooleanField)
                    _RAWithPolyneuropathy = value
                End Set
            End Property

            Private _RAWithMyopathy As New BooleanField
            <DataMember()>
            Public Property RAWithMyopathy() As BooleanField
                Get
                    Return _RAWithMyopathy
                End Get
                Set(ByVal value As BooleanField)
                    _RAWithMyopathy = value
                End Set
            End Property

            Private _RAOtherManifestations As New StringField
            <DataMember()>
            Public Property RAOtherManifestations() As StringField
                Get
                    Return _RAOtherManifestations
                End Get
                Set(ByVal value As StringField)
                    _RAOtherManifestations = value
                End Set
            End Property


            Private _DMARDs As New BooleanField
            <DataMember()>
            Public Property DMARDs() As BooleanField
                Get
                    Return _DMARDs
                End Get
                Set(ByVal value As BooleanField)
                    _DMARDs = value
                End Set
            End Property

            Private _DMARDsSpecify As New StringField
            <DataMember()>
            Public Property DMARDsSpecify() As StringField
                Get
                    Return _DMARDsSpecify
                End Get
                Set(ByVal value As StringField)
                    _DMARDsSpecify = value
                End Set
            End Property

            Private _PtRefuses As New BooleanField
            <DataMember()>
            Public Property PtRefuese() As BooleanField
                Get
                    Return _PtRefuses
                End Get
                Set(ByVal value As BooleanField)
                    _PtRefuses = value
                End Set
            End Property

            Private _OtherTreatmentConditions As New StringField
            <DataMember()>
            Public Property OtherTreatmentConditions() As StringField
                Get
                    Return _OtherTreatmentConditions
                End Get
                Set(ByVal value As StringField)
                    _OtherTreatmentConditions = value
                End Set
            End Property



            Private _Osteoporosis As New BooleanField
            <DataMember()>
            Public Property Osteoporosis() As BooleanField
                Get
                    Return _Osteoporosis
                End Get
                Set(ByVal value As BooleanField)
                    _Osteoporosis = value
                End Set
            End Property


            Private _Osteopenia As New BooleanField
            <DataMember()>
            Public Property Osteopenia() As BooleanField
                Get
                    Return _Osteopenia
                End Get
                Set(ByVal value As BooleanField)
                    _Osteopenia = value
                End Set
            End Property

            <DataMember()>
            Public Property Arthritis As New BooleanField
            <DataMember()>
            Public Property ArthritisLocationType As New StringField
            <DataMember()>
            Public Property NSAIDS As New BooleanField
            <DataMember>
            Public Property NSAIDSOtherTreatment As New StringField
            <DataMember()>
            Public Property AffectedJoints As New StringField
            <DataMember()>
            Public Property NA As Boolean
            <DataMember()>
            Public Property InflammatoryPolyarthritis As New BooleanField
            <DataMember()>
            Public Property InflammatoryPolyarthritisComments As New StringField
            <DataMember()>
            Public Property ArthropathySequelaViralInfection As New BooleanField
            <DataMember()>
            Public Property ArthropathySequelaViralInfectionComments As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)
            <DataMember()>
            Public Property RheumatoidArthritis_Osteoartritis As New BooleanField
            <DataMember()>
            Public Property RheumatoidArthritis_ArtritisPsoriatrica As New BooleanField
            <DataMember()>
            Public Property RheumatoidArthritis_OsteoartritisComment As New StringField
            <DataMember()>
            Public Property RheumatoidArthritis_ArtritisPsoriatricaComment As New StringField
            <DataMember()>
            Public Property RAOtherManifestationsCheckBox As New BooleanField

            <DataMember()>
            Public Property OsteoTreatmentPlan As New StringField
        End Class

#End Region

#Region " Major Depression"

        <Serializable(), DataContract()>
        Public Class MajorDepressionSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "MD"
                Me.NA = False
                Me._IsMajorDepression = New BooleanField() With {.Value = False}
                Me._InRemission = New BooleanField() With {.Value = False}
                Me._Recurrent = New BooleanField() With {.Value = False}
                Me._MildSeverity = New BooleanField() With {.Value = False}
                Me._ModerateSeverity = New BooleanField() With {.Value = False}
                Me._SevereSeverity = New BooleanField() With {.Value = False}
                Me._TreatmentPlan = New StringField()
                Me.SingleEpisode = New BooleanField() With {.Value = False}
                Me.PsychoticSymptoms = New BooleanField() With {.Value = False}
                Me.BipolarDisorder = New BooleanField() With {.Value = False}
                Me.BipolarDisorderTypeAndSeverity = New StringField()
                Me.BipolarDisorderTreatmentPlan = New StringField()
                Me.UseOfSubtancesTreatmentPlan = New StringField()
                Me.UseOfSubtancesListCount = New Integer()
                Me.ScreeningSubstanceUseListResult = New StringField()
                Me.Schizophrenia = New BooleanField() With {.Value = False}
                Me.SchizophreniaType = New StringField()
                Me.SchizophreniaTreatmentPlan = New StringField()
                Me.MoodDisorder = New BooleanField() With {.Value = False}
                Me.MoodDisorderComments = New StringField()
                Me.PHQ9DoneDate = New DateField()
                Me.PHQ9ScoreResult = New StringField()
                Me.Dysthymia = New BooleanField() With {.Value = False}
                Me.DysthymiaComments = New StringField()
                Me.PHQ9 = New PHQ9()
                Me.ADHD = New BooleanField() With {.Value = False}
                Me.ADHDComments = New StringField()
                Me.Autism = New BooleanField() With {.Value = False}
                Me.AutismComments = New StringField()
                Me.PHQ9ReasonNotDoneID = New IntegerField()
                Me.PHQ9ReasonNotDoneOther = New StringField()
                Me.MedicationList = New List(Of String)()
                Me.MentalHealth_SubstanceAbuseFreeText = New StringField()
                Me.MentalHealth_SubstanceAbuseCheckBox = New BooleanField() With {.Value = False}
                Me.MentalHealth_SeverWithoutPsychoticSymptoms = New BooleanField() With {.Value = False}
                Me.MentalHealth_ScreeningSubstanceUseDatePerformed = New DateField
                Me.GeneralizedAnxietyDisorder = New BooleanField()
                Me.OtherAnxiety = New BooleanField()
                Me.OtherAnxietyText = New StringField()
                Me.GeneralizedAnxietyDisorderComments = New StringField()
            End Sub

            <DataMember()>
            Public Property NA As Boolean

            Private _IsMajorDepression As New BooleanField
            <DataMember()>
            Public Property IsMajorDepression() As BooleanField
                Get
                    Return _IsMajorDepression
                End Get
                Set(ByVal value As BooleanField)
                    _IsMajorDepression = value
                End Set
            End Property

            Private _InRemission As New BooleanField
            <DataMember()>
            Public Property InRemission() As BooleanField
                Get
                    Return _InRemission
                End Get
                Set(ByVal value As BooleanField)
                    _InRemission = value
                End Set
            End Property

            Private _Recurrent As New BooleanField
            <DataMember()>
            Public Property Recurrent() As BooleanField
                Get
                    Return _Recurrent
                End Get
                Set(ByVal value As BooleanField)
                    _Recurrent = value
                End Set
            End Property

            Private _MildSeverity As New BooleanField
            <DataMember()>
            Public Property MildSeverity() As BooleanField
                Get
                    Return _MildSeverity
                End Get
                Set(ByVal value As BooleanField)
                    _MildSeverity = value
                End Set
            End Property

            Private _ModerateSeverity As New BooleanField
            <DataMember()>
            Public Property ModerateSeverity() As BooleanField
                Get
                    Return _ModerateSeverity
                End Get
                Set(ByVal value As BooleanField)
                    _ModerateSeverity = value
                End Set
            End Property

            Private _SevereSeverity As New BooleanField
            <DataMember()>
            Public Property SevereSeverity() As BooleanField
                Get
                    Return _SevereSeverity
                End Get
                Set(ByVal value As BooleanField)
                    _SevereSeverity = value
                End Set
            End Property

            Private _TreatmentPlan As New StringField
            <DataMember()>
            Public Property TreatmentPlan() As StringField
                Get
                    Return _TreatmentPlan
                End Get
                Set(ByVal value As StringField)
                    _TreatmentPlan = value
                End Set
            End Property

            <DataMember()>
            Public Property SingleEpisode As New BooleanField
            <DataMember()>
            Public Property PsychoticSymptoms As New BooleanField
            <DataMember()>
            Public Property BipolarDisorder As New BooleanField
            <DataMember()>
            Public Property BipolarDisorderTypeAndSeverity As New StringField
            <DataMember()>
            Public Property BipolarDisorderTreatmentPlan As New StringField
            <DataMember()>
            Public Property UseOfSubtancesTreatmentPlan As New StringField
            <DataMember()>
            Public Property UseOfSubtancesListCount As New Integer
            <DataMember()>
            Public Property ScreeningSubstanceUseListResult As New StringField
            <DataMember()>
            Public Property Schizophrenia As New BooleanField
            <DataMember()>
            Public Property SchizophreniaType As New StringField
            <DataMember()>
            Public Property SchizophreniaTreatmentPlan As New StringField
            <DataMember()>
            Public Property MoodDisorder As New BooleanField
            <DataMember()>
            Public Property MoodDisorderComments As New StringField
            <DataMember()>
            Public Property PHQ9DoneDate As New DateField
            <DataMember()>
            Public Property PHQ9ScoreResult As New StringField
            <DataMember()>
            Public Property Dysthymia As New BooleanField
            <DataMember()>
            Public Property DysthymiaComments As New StringField
            <DataMember()>
            Public Property PHQ9 As New PHQ9
            <DataMember()>
            Public Property ADHD As New BooleanField
            <DataMember>
            Public Property ADHDComments As New StringField
            <DataMember()>
            Public Property Autism As New BooleanField
            <DataMember()>
            Public Property AutismComments As New StringField
            <DataMember()>
            Public Property PHQ9ReasonNotDoneID As New IntegerField
            <DataMember()>
            Public Property PHQ9ReasonNotDoneOther As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)
            <DataMember()>
            Public Property MentalHealth_SubstanceAbuseFreeText As New StringField
            <DataMember()>
            Public Property MentalHealth_SubstanceAbuseCheckBox As New BooleanField
            <DataMember()>
            Public Property MentalHealth_SeverWithoutPsychoticSymptoms As New BooleanField
            <DataMember()>
            Public Property MentalHealth_ScreeningSubstanceUseDatePerformed As New DateField


            <DataMember()>
            Public Property GeneralizedAnxietyDisorder As New BooleanField

            <DataMember()>
            Public Property OtherAnxiety As New BooleanField

            <DataMember()>
            Public Property OtherAnxietyText As New StringField
            <DataMember()>
            Public Property GeneralizedAnxietyDisorderComments As New StringField


        End Class

#End Region

#Region " Depression Inventory"

        <Serializable(), DataContract()>
        Public Class DepressionInventorySection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "DI"
                Me.PlanOfTreatment = New StringField() With {.Value = ""}
            End Sub

            Private _Choose1 As New IntegerField
            <DataMember()>
            Public Property Choose1() As IntegerField
                Get
                    Return _Choose1
                End Get
                Set(ByVal value As IntegerField)
                    _Choose1 = value
                End Set
            End Property

            Private _Choose2 As New IntegerField
            <DataMember()>
            Public Property Choose2() As IntegerField
                Get
                    Return _Choose2
                End Get
                Set(ByVal value As IntegerField)
                    _Choose2 = value
                End Set
            End Property

            Private _Choose3 As New IntegerField
            <DataMember()>
            Public Property Choose3() As IntegerField
                Get
                    Return _Choose3
                End Get
                Set(ByVal value As IntegerField)
                    _Choose3 = value
                End Set
            End Property

            Private _Choose4 As New IntegerField
            <DataMember()>
            Public Property Choose4() As IntegerField
                Get
                    Return _Choose4
                End Get
                Set(ByVal value As IntegerField)
                    _Choose4 = value
                End Set
            End Property

            Private _Choose5 As New IntegerField
            <DataMember()>
            Public Property Choose5() As IntegerField
                Get
                    Return _Choose5
                End Get
                Set(ByVal value As IntegerField)
                    _Choose5 = value
                End Set
            End Property

            Private _Choose6 As New IntegerField
            <DataMember()>
            Public Property Choose6() As IntegerField
                Get
                    Return _Choose6
                End Get
                Set(ByVal value As IntegerField)
                    _Choose6 = value
                End Set
            End Property

            Private _Choose7 As New IntegerField
            <DataMember()>
            Public Property Choose7() As IntegerField
                Get
                    Return _Choose7
                End Get
                Set(ByVal value As IntegerField)
                    _Choose7 = value
                End Set
            End Property

            Private _Choose8a As New IntegerField
            <DataMember()>
            Public Property Choose8a() As IntegerField
                Get
                    Return _Choose8a
                End Get
                Set(ByVal value As IntegerField)
                    _Choose8a = value
                End Set
            End Property

            Private _Choose8b As New IntegerField
            <DataMember()>
            Public Property Choose8b() As IntegerField
                Get
                    Return _Choose8b
                End Get
                Set(ByVal value As IntegerField)
                    _Choose8b = value
                End Set
            End Property

            Private _Choose9 As New IntegerField
            <DataMember()>
            Public Property Choose9() As IntegerField
                Get
                    Return _Choose9
                End Get
                Set(ByVal value As IntegerField)
                    _Choose9 = value
                End Set
            End Property

            Private _Choose10a As New IntegerField
            <DataMember()>
            Public Property Choose10a() As IntegerField
                Get
                    Return _Choose10a
                End Get
                Set(ByVal value As IntegerField)
                    _Choose10a = value
                End Set
            End Property

            Private _Choose10b As New IntegerField
            <DataMember()>
            Public Property Choose10b() As IntegerField
                Get
                    Return _Choose10b
                End Get
                Set(ByVal value As IntegerField)
                    _Choose10b = value
                End Set
            End Property

            Private _IsMild As New BooleanField
            <DataMember()>
            Public Property IsMild() As BooleanField
                Get
                    Return _IsMild
                End Get
                Set(ByVal value As BooleanField)
                    _IsMild = value
                End Set
            End Property

            Private _IsSevere As New BooleanField
            <DataMember()>
            Public Property IsSevere() As BooleanField
                Get
                    Return _IsSevere
                End Get
                Set(ByVal value As BooleanField)
                    _IsSevere = value
                End Set
            End Property

            Private _IsMajor As New BooleanField
            <DataMember()>
            Public Property IsMajor() As BooleanField
                Get
                    Return _IsMajor
                End Get
                Set(ByVal value As BooleanField)
                    _IsMajor = value
                End Set
            End Property

            Private _IsModerate As New BooleanField
            <DataMember()>
            Public Property IsModerate() As BooleanField
                Get
                    Return _IsModerate
                End Get
                Set(ByVal value As BooleanField)
                    _IsModerate = value
                End Set
            End Property

            Private _PlanOfTreatment As New StringField
            <DataMember()>
            Public Property PlanOfTreatment() As StringField
                Get
                    Return _PlanOfTreatment
                End Get
                Set(ByVal value As StringField)
                    _PlanOfTreatment = value
                End Set
            End Property

            <DataMember()>
            Public Property MedicationList As New List(Of String)

        End Class

#End Region

#Region " DME Use"

        <Serializable(), DataContract()>
        Public Class DMEUseSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "DME"
            End Sub

            Private _UsingOxygen As New BooleanField
            <DataMember()>
            Public Property UsingOxygen() As BooleanField
                Get
                    Return _UsingOxygen
                End Get
                Set(ByVal value As BooleanField)
                    _UsingOxygen = value
                End Set
            End Property

            Private _DueToHypoxiaInAir As New BooleanField
            <DataMember()>
            Public Property DueToHypoxiaInAir() As BooleanField
                Get
                    Return _DueToHypoxiaInAir
                End Get
                Set(ByVal value As BooleanField)
                    _DueToHypoxiaInAir = value
                End Set
            End Property

            Private _CPAP As New BooleanField
            <DataMember()>
            Public Property CPAP() As BooleanField
                Get
                    Return _CPAP
                End Get
                Set(ByVal value As BooleanField)
                    _CPAP = value
                End Set
            End Property

            Private _AboveKneeProsthesis As New BooleanField
            <DataMember()>
            Public Property AboveKneeProsthesis() As BooleanField
                Get
                    Return _AboveKneeProsthesis
                End Get
                Set(ByVal value As BooleanField)
                    _AboveKneeProsthesis = value
                End Set
            End Property

            Private _BelowKneeProsthesis As New BooleanField
            <DataMember()>
            Public Property BelowKneeProsthesis() As BooleanField
                Get
                    Return _BelowKneeProsthesis
                End Get
                Set(ByVal value As BooleanField)
                    _BelowKneeProsthesis = value
                End Set
            End Property

            Private _HasSuppliesNeeded As New BooleanField
            <DataMember()>
            Public Property HasSuppliesNeeded() As BooleanField
                Get
                    Return _HasSuppliesNeeded
                End Get
                Set(ByVal value As BooleanField)
                    _HasSuppliesNeeded = value
                End Set
            End Property

            Private _Gastrostomy As New BooleanField
            <DataMember()>
            Public Property Gastrostomy() As BooleanField
                Get
                    Return _Gastrostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Gastrostomy = value
                End Set
            End Property

            Private _Colostomy As New BooleanField
            <DataMember()>
            Public Property Colostomy() As BooleanField
                Get
                    Return _Colostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Colostomy = value
                End Set
            End Property

            Private _Urostomy As New BooleanField
            <DataMember()>
            Public Property Urostomy() As BooleanField
                Get
                    Return _Urostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Urostomy = value
                End Set
            End Property

            Private _Tracheostomy As New BooleanField
            <DataMember()>
            Public Property Tracheostomy() As BooleanField
                Get
                    Return _Tracheostomy
                End Get
                Set(ByVal value As BooleanField)
                    _Tracheostomy = value
                End Set
            End Property

            Private _UsingWheelchair As New BooleanField
            <DataMember()>
            Public Property UsingWheelchair() As BooleanField
                Get
                    Return _UsingWheelchair
                End Get
                Set(ByVal value As BooleanField)
                    _UsingWheelchair = value
                End Set
            End Property

            Private _UsingWheelchairReason As New StringField
            <DataMember()>
            Public Property UsingWheelchairReason() As StringField
                Get
                    Return _UsingWheelchairReason
                End Get
                Set(ByVal value As StringField)
                    _UsingWheelchairReason = value
                End Set
            End Property

            Private _Comments As New StringField
            <DataMember()>
            Public Property Comments() As StringField
                Get
                    Return _Comments
                End Get
                Set(ByVal value As StringField)
                    _Comments = value
                End Set
            End Property

        End Class

#End Region

#Region " Historial del Infarto del Miocardio (Old MI)"

        <Serializable(), DataContract()>
        Public Class MyocardialInfarctionSection
            Inherits AHASectionErrorSummary


            Private _OldMI As New BooleanField
            <DataMember()>
            Public Property OldMI() As BooleanField
                Get
                    Return _OldMI
                End Get
                Set(ByVal value As BooleanField)
                    _OldMI = value
                End Set
            End Property

            Private _BetaBlocker As New BooleanField
            <DataMember()>
            Public Property BetaBlocker() As BooleanField
                Get
                    Return _BetaBlocker
                End Get
                Set(ByVal value As BooleanField)
                    _BetaBlocker = value
                End Set
            End Property

            Private _BetaBlockerType As New StringField
            <DataMember()>
            Public Property BetaBlockerType() As StringField
                Get
                    Return _BetaBlockerType
                End Get
                Set(ByVal value As StringField)
                    _BetaBlockerType = value
                End Set
            End Property

            Private _OtherTreatmentCircumstances As New StringField
            <DataMember()>
            Public Property OtherTreatmentCircumstances() As StringField
                Get
                    Return _OtherTreatmentCircumstances
                End Get
                Set(ByVal value As StringField)
                    _OtherTreatmentCircumstances = value
                End Set
            End Property

            Private _AMI6Months As New BooleanField
            <DataMember()>
            Public Property AMI6Months() As BooleanField
                Get
                    Return _AMI6Months
                End Get
                Set(value As BooleanField)
                    _AMI6Months = value
                End Set
            End Property
        End Class

#End Region

#Region " BMI Associated Diagnoses"

        <Serializable(), DataContract()>
        Public Class BMIAssociatedDiagnosesSection
            Inherits AHASectionErrorSummary

            Private _Obesity As New BooleanField
            <DataMember()>
            Public Property Obesity() As BooleanField
                Get
                    Return _Obesity
                End Get
                Set(ByVal value As BooleanField)
                    _Obesity = value
                End Set
            End Property

            Private _MorbidObesity As New BooleanField
            <DataMember()>
            Public Property MorbidObesity() As BooleanField
                Get
                    Return _MorbidObesity
                End Get
                Set(ByVal value As BooleanField)
                    _MorbidObesity = value
                End Set
            End Property

            Private _Malnutrition As New BooleanField
            <DataMember()>
            Public Property Malnutrition() As BooleanField
                Get
                    Return _Malnutrition
                End Get
                Set(ByVal value As BooleanField)
                    _Malnutrition = value
                End Set
            End Property

            Private _EvaluationTreatmentPlan As New StringField
            <DataMember()>
            Public Property EvaluationTreatmentPlan() As StringField
                Get
                    Return _EvaluationTreatmentPlan
                End Get
                Set(ByVal value As StringField)
                    _EvaluationTreatmentPlan = value
                End Set
            End Property

            <DataMember()>
            Public Property MalnutritionGradeTypeText As New StringField
            <DataMember()>
            Public Property NA As Boolean
            <DataMember()>
            Public Property DeficiencyVitaminB12 As New BooleanField
            <DataMember()>
            Public Property DeficiencyOtherVitaminNutrients As New BooleanField
            <DataMember()>
            Public Property DeficiencyOtherVitaminNutrientsComments As New StringField
            <DataMember()>
            Public Property DeficiencyBComplex As New BooleanField

            <DataMember()>
            Public Property DeficiencyVitaminB6 As New BooleanField

            <DataMember()>
            Public Property MalnutritionScreeningAssesment As New StringField

            <DataMember()>
            Public Property MedicationList As New List(Of String)

        End Class

#End Region

#Region " Enfermedades de la piel"

        <Serializable(), DataContract()>
        Public Class DiseasesOfTheSkin
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.Dermatitis = New BooleanField()
                Me.DermatitisTreatment = New StringField()
                Me.DermatitisTypeLocation = New StringField()
                Me.Psoriasis = New BooleanField()
                Me.PsoriasisType = New StringField()
                Me.PsoriasicArthritis = New BooleanField()
                Me.PsoriasicArthritisLocation = New StringField()
                Me.PsoriasisTreatment = New StringField()
                Me.RheumatoidArthritis_Osteoartritis = New BooleanField()
                Me.RheumatoidArthritis_OsteoartritisComment = New StringField()
                Me.RheumatoidArthritis_ArtritisPsoriatrica = New BooleanField()
                Me.RheumatoidArthritis_ArtritisPsoriatricaComment = New StringField()
                Me.Ulcer = New BooleanField()
                Me.UlcerLocationAndDepth = New StringField()
                Me.UlcerLocation = New StringField()
                Me.UlcerDepth = New StringField()
                Me.DueToArteriosclerosisInExtremities = New BooleanField()
                Me.DueToPVD = New BooleanField()
                Me.UlcerTreatment = New StringField()
                Me.PressureUlcer = New BooleanField()
                Me.PressureUlcerStage = New IntegerField()
                Me.PressureUlcerNoStage = New BooleanField()
                Me.PressureUlcerLocation = New StringField()
                Me.PressureUlcerTreatment = New StringField()
                Me.PressureUlcerOtherCause = New BooleanField()
                Me.PressureUlcerOtherCauseText = New StringField()
                Me.PressureUlcerOtherCauseTreatment = New StringField()
                Me.MedicationList = New List(Of String)
                Me.UlcerDueToDiabetes = New BooleanField()
                Me.UlcerDueToVaricoseVeins = New BooleanField()
                Me.UlcerDueToVaricoseVeinsWithInflamation = New BooleanField()
                Me.UlcerDueToIdiopathicVenousHypertension = New BooleanField()
                Me.UlcerDueToIdiopathicVenousHypertensionWithInflamation = New BooleanField()
                Me.UlcerDueToOtherCause = New BooleanField()
                Me.UlcerDueToOtherCauseText = New StringField()
            End Sub

            Public Property biClaimID As Long
            Public Property IndexRow As Integer

            <DataMember()>
            Public Property Dermatitis As New BooleanField
            <DataMember()>
            Public Property DermatitisTreatment As New StringField
            <DataMember()>
            Public Property DermatitisTypeLocation As New StringField
            <DataMember()>
            Public Property Psoriasis As New BooleanField
            <DataMember()>
            Public Property PsoriasisType As New StringField
            <DataMember()>
            Public Property PsoriasicArthritis As New BooleanField
            <DataMember()>
            Public Property PsoriasicArthritisLocation As New StringField
            <DataMember()>
            Public Property PsoriasisTreatment As New StringField
            <DataMember()>
            Public Property RheumatoidArthritis_Osteoartritis As New BooleanField
            <DataMember()>
            Public Property RheumatoidArthritis_OsteoartritisComment As New StringField
            <DataMember()>
            Public Property RheumatoidArthritis_ArtritisPsoriatrica As New BooleanField
            <DataMember()>
            Public Property RheumatoidArthritis_ArtritisPsoriatricaComment As New StringField
            <DataMember()>
            Public Property Ulcer As New BooleanField
            <DataMember()>
            Public Property UlcerLocationAndDepth As New StringField
            <DataMember()>
            Public Property UlcerLocation As New StringField
            <DataMember()>
            Public Property UlcerDepth As New StringField
            <DataMember()>
            Public Property DueToArteriosclerosisInExtremities As New BooleanField
            <DataMember()>
            Public Property DueToPVD As New BooleanField
            <DataMember()>
            Public Property UlcerTreatment As New StringField
            <DataMember()>
            Public Property PressureUlcer As New BooleanField
            <DataMember()>
            Public Property PressureUlcerStage As New IntegerField
            <DataMember()>
            Public Property PressureUlcerNoStage As New BooleanField
            <DataMember()>
            Public Property PressureUlcerLocation As New StringField
            <DataMember()>
            Public Property PressureUlcerTreatment As New StringField
            <DataMember()>
            Public Property PressureUlcerOtherCause As New BooleanField
            <DataMember()>
            Public Property PressureUlcerOtherCauseText As New StringField
            <DataMember()>
            Public Property PressureUlcerOtherCauseTreatment As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)
            <DataMember()>
            Public Property UlcerDueToDiabetes As BooleanField
            <DataMember()>
            Public Property UlcerDueToVaricoseVeins As BooleanField
            <DataMember()>
            Public Property UlcerDueToVaricoseVeinsWithInflamation As BooleanField
            <DataMember()>
            Public Property UlcerDueToIdiopathicVenousHypertension As BooleanField
            <DataMember()>
            Public Property UlcerDueToIdiopathicVenousHypertensionWithInflamation As BooleanField
            <DataMember()>
            Public Property UlcerDueToOtherCause As BooleanField
            <DataMember()>
            Public Property UlcerDueToOtherCauseText As StringField
            <DataMember()>
            Public Property UlcerDueToPVDWithInflamation As BooleanField
        End Class

#End Region

#Region " Enfermedades cardiovasculares"

        <Serializable(), DataContract()>
        Public Class CardiovascularDiseases
            Inherits AHASectionErrorSummary

            Public Sub New()

            End Sub

            <DataMember()>
            Public Property NA As New BooleanField
            <DataMember()>
            Public Property ArterialHypertension As New BooleanField
            <DataMember()>
            Public Property PulmonaryHypertension As New BooleanField
            <DataMember()>
            Public Property PulmonaryHypertensionType As New StringField
            <DataMember()>
            Public Property HeartFailure As New BooleanField
            <DataMember()>
            Public Property Congestive As New BooleanField
            <DataMember()>
            Public Property Diastolic As New BooleanField
            <DataMember()>
            Public Property Systolic As New BooleanField
            <DataMember()>
            Public Property Chronic As New BooleanField
            <DataMember()>
            Public Property PVD As New BooleanField
            <DataMember()>
            Public Property AtrilaFibrillation As New BooleanField
            <DataMember()>
            Public Property AtrilFibrillationType As New StringField
            <DataMember()>
            Public Property Arteriosclerosis As New BooleanField
            <DataMember()>
            Public Property Aorta As New BooleanField
            <DataMember()>
            Public Property Crowns As New BooleanField
            <DataMember()>
            Public Property RenalArtery As New BooleanField
            <DataMember()>
            Public Property AnginaPectoris As New BooleanField
            <DataMember()>
            Public Property ArteriosclerosisExtremities As New BooleanField
            <DataMember()>
            Public Property LegLT As New BooleanField
            <DataMember()>
            Public Property LegRT As New BooleanField
            <DataMember()>
            Public Property ArmLT As New BooleanField
            <DataMember()>
            Public Property ArmRT As New BooleanField
            <DataMember()>
            Public Property IntermittentClaudication As New BooleanField
            <DataMember()>
            Public Property RestPain As New BooleanField
            <DataMember()>
            Public Property OtherComplications As New BooleanField
            <DataMember()>
            Public Property OtherComplicationsText As New StringField
            <DataMember()>
            Public Property HypertensionTreatmentPlan As New StringField
            <DataMember()>
            Public Property PVDTreatmentPlan As New StringField
            <DataMember()>
            Public Property ArteriosclerosisTreatmentPlan As New StringField
            <DataMember()>
            Public Property SSS As New BooleanField
            <DataMember()>
            Public Property SVT As New BooleanField
            <DataMember()>
            Public Property Pacemaker As New BooleanField
            <DataMember()>
            Public Property CAD As New BooleanField
            <DataMember()>
            Public Property Cardiomiopatia As New BooleanField
            <DataMember()>
            Public Property MedicationList As New List(Of String)

            <DataMember()>
            Public Property OldMyocardialInfarction As New BooleanField
            <DataMember()>
            Public Property Hyperlipidemia As New BooleanField

            <DataMember()>
            Public Property HyperlipidemiaText As New StringField

            <DataMember()>
            Public Property HeartTransplant As New BooleanField




            <DataMember()>
            Public Property MyocardialInfarction As New BooleanField

            <DataMember()>
            Public Property Cardiomegaly As New BooleanField

            <DataMember()>
            Public Property AtrioventricularBlock As New BooleanField

            <DataMember()>
            Public Property AtrioventricularBlockDegree As New StringField

            <DataMember()>
            Public Property VaricoseVeinsOfLowerExtremityWithPain As New BooleanField

            <DataMember()>
            Public Property ConductionDisorder As New BooleanField

            <DataMember()>
            Public Property MyocardialInfarctionTreatmentPlan As New StringField

        End Class

#End Region

#Region " Ojos y Neurologia"

        <Serializable(), DataContract()>
        Public Class EyesAndNeurology
            Inherits AHASectionErrorSummary

            Public Sub New()
            End Sub

            <DataMember()>
            Public Property NA As New BooleanField
            <DataMember()>
            Public Property Retinopathy As New BooleanField
            <DataMember()>
            Public Property Proliferative As New BooleanField
            <DataMember()>
            Public Property ProliferativeEyeRT As New BooleanField
            <DataMember()>
            Public Property ProliferativeEyeLT As New BooleanField
            <DataMember()>
            Public Property MacularEdema As New BooleanField
            <DataMember()>
            Public Property MacularEdemaEyeRT As New BooleanField
            <DataMember()>
            Public Property MacularEdemaEyeLT As New BooleanField
            <DataMember()>
            Public Property OtherComplicationRetinopathy As New StringField
            <DataMember()>
            Public Property Glaucoma As New BooleanField
            <DataMember()>
            Public Property GlaucomaEyeRT As New BooleanField
            <DataMember()>
            Public Property GlaucomaEyeLT As New BooleanField
            <DataMember()>
            Public Property GlaucomaType As New StringField
            <DataMember()>
            Public Property Cataract As New BooleanField
            <DataMember()>
            Public Property CataractRT As New BooleanField
            <DataMember()>
            Public Property CataractLT As New BooleanField
            <DataMember()>
            Public Property CataractType As New StringField
            <DataMember()>
            Public Property Epilepsy As New BooleanField
            <DataMember()>
            Public Property EpilepsyType As New StringField
            <DataMember()>
            Public Property Seizures As New BooleanField
            <DataMember()>
            Public Property SeizuresCause As New StringField
            <DataMember()>
            Public Property Polyneuropathy As New BooleanField
            <DataMember()>
            Public Property PolyneuropathyDueToCkb As New BooleanField
            <DataMember()>
            Public Property PolyneuropathyDueTo As New StringField
            <DataMember()>
            Public Property Neuropathy As New BooleanField
            <DataMember()>
            Public Property AutonomicNeuropathy As New BooleanField
            <DataMember()>
            Public Property Mononeuritis As New BooleanField
            <DataMember()>
            Public Property Neuralgia As New BooleanField
            <DataMember()>
            Public Property PolyneuropathyOtherSpecification As New StringField
            <DataMember()>
            Public Property RetinopathyTreatmentPlan As New StringField
            <DataMember()>
            Public Property GlaucomaTreatmentPlan As New StringField
            <DataMember()>
            Public Property CataractTreatmentPlan As New StringField
            <DataMember()>
            Public Property EpilepsyTreatmentPlan As New StringField
            <DataMember()>
            Public Property PolyneuropathyTreatmentPlan As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)

            'ADD 2026 Changes'

            <DataMember()>
            Public Property RetinopathySeverity As New IntegerField

            <DataMember()>
            Public Property ProliferativeSeverity As New IntegerField

            <DataMember()>
            Public Property ProliferativeTreatmentPlan As New StringField

            <DataMember()>
            Public Property RetinopathyEyeRT As New BooleanField

            <DataMember()>
            Public Property RetinopathyEyeLT As New BooleanField


            <DataMember()>
            Public Property AlzheimerDisease As New BooleanField


            <DataMember()>
            Public Property Dementia As New BooleanField


            <DataMember()>
            Public Property DementiaSeverity As New IntegerField




            <DataMember()>
            Public Property DementiaAlzheimerTreatmentPlan As New StringField


        End Class

#End Region

#Region " IMLABRef"

        <Serializable(), DataContract()>
        Public Class ImLabRef
            Inherits AHASectionErrorSummary

            Public Sub New()

                Immunizations_NA = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_ParentRefuses = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HepB1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HebB2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HebB3dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HepA1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HebA2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_DTaP1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_DTaP2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_DTaP3dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_DTaP4dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_DTaP5dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Hib1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Hib2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Hib3dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Hib4dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_PCV13_1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_PCV13_2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_PCV13_3dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_PCV13_4dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_IPV1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_IPV2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_IPV3dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_IPV4dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_MMR1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_MMR2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Varicella1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Varicella2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Tdap = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Rotavirus1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Rotavirus2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Influenza = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_MenningococcalMCV = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HPV1dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HPV2dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_HPV3dose = New BooleanField() With {.Value = False, .HasError = False}
                Immunizations_Others = New StringField() With {.Value = "", .HasError = False}

                Lab_NA = New BooleanField() With {.Value = False, .HasError = False}
                Lab_HgbHct = New BooleanField() With {.Value = False, .HasError = False}
                Lab_TB = New BooleanField() With {.Value = False, .HasError = False}
                Lab_UA = New BooleanField() With {.Value = False, .HasError = False}
                Lab_LipidProfile = New BooleanField() With {.Value = False, .HasError = False}
                Lab_BloodLeadTest = New BooleanField() With {.Value = False, .HasError = False}
                Lab_VIH = New BooleanField() With {.Value = False, .HasError = False}
                Lab_NAAT = New BooleanField() With {.Value = False, .HasError = False}
                Lab_VDRL = New BooleanField() With {.Value = False, .HasError = False}
                Lab_Other = New StringField() With {.Value = "", .HasError = False}

                VisionText = New StringField() With {.Value = "", .HasError = False}
                HearingText = New StringField() With {.Value = "", .HasError = False}

                Referrals_NA = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_WIC = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_PhysicalTherapy = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_OccupationTherapy = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_SpeechTherapy = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_Audiology = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_Dental = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_BehavioralHealth = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_EarlyIntervention = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_MentalHealthSpecialist = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_Nutritionist = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_Optometrist = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_Ophthalmology = New BooleanField() With {.Value = False, .HasError = False}
                Referrals_OtherSpecialtyText = New StringField() With {.Value = "", .HasError = False}

                Immuno_Others_Checkbox = New BooleanField With {.Value = False, .HasError = False}
                Labs_Others_Checkbox = New BooleanField With {.Value = False, .HasError = False}
                Referals_Others_Checkbox = New BooleanField With {.Value = False, .HasError = False}

                Lab_HgbHct_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_TB_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_UA_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_LipidProfile_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_BloodLeadTest_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_VIH_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_NAAT_Ordered = New BooleanField() With {.Value = False, .HasError = False}
                Lab_VDRL_Ordered = New BooleanField() With {.Value = False, .HasError = False}

                Lab_HgbHct_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_TB_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_UA_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_LipidProfile_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_BloodLeadTest_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_VIH_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_NAAT_Result = New StringField() With {.Value = "", .HasError = False}
                Lab_VDRL_Result = New StringField() With {.Value = "", .HasError = False}
            End Sub

            <DataMember()>
            Public Property Immunizations_NA As New BooleanField
            <DataMember()>
            Public Property Immunizations_ParentRefuses As New BooleanField
            <DataMember()>
            Public Property Immunizations_HepB1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_HebB2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_HebB3dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_HepA1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_HebA2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_DTaP1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_DTaP2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_DTaP3dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_DTaP4dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_DTaP5dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Hib1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Hib2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Hib3dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Hib4dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_PCV13_1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_PCV13_2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_PCV13_3dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_PCV13_4dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_IPV1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_IPV2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_IPV3dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_IPV4dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_MMR1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_MMR2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Varicella1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Varicella2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Tdap As New BooleanField
            <DataMember()>
            Public Property Immunizations_Rotavirus1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Rotavirus2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Influenza As New BooleanField
            <DataMember()>
            Public Property Immunizations_MenningococcalMCV As New BooleanField
            <DataMember()>
            Public Property Immunizations_HPV1dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_HPV2dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_HPV3dose As New BooleanField
            <DataMember()>
            Public Property Immunizations_Others As New StringField
            <DataMember()>
            Public Property Lab_NA As New BooleanField
            <DataMember()>
            Public Property Lab_HgbHct As New BooleanField
            <DataMember()>
            Public Property Lab_TB As New BooleanField
            <DataMember()>
            Public Property Lab_UA As New BooleanField
            <DataMember()>
            Public Property Lab_LipidProfile As New BooleanField
            <DataMember()>
            Public Property Lab_BloodLeadTest As New BooleanField
            <DataMember()>
            Public Property Lab_VIH As New BooleanField
            <DataMember()>
            Public Property Lab_NAAT As New BooleanField
            <DataMember()>
            Public Property Lab_VDRL As New BooleanField
            <DataMember()>
            Public Property Lab_Other As New StringField
            <DataMember()>
            Public Property VisionText As New StringField
            <DataMember()>
            Public Property HearingText As New StringField
            <DataMember()>
            Public Property Referrals_NA As New BooleanField
            <DataMember()>
            Public Property Referrals_WIC As New BooleanField
            <DataMember()>
            Public Property Referrals_PhysicalTherapy As New BooleanField
            <DataMember()>
            Public Property Referrals_OccupationTherapy As New BooleanField
            <DataMember()>
            Public Property Referrals_SpeechTherapy As New BooleanField
            <DataMember()>
            Public Property Referrals_Audiology As New BooleanField
            <DataMember()>
            Public Property Referrals_Dental As New BooleanField
            <DataMember()>
            Public Property Referrals_BehavioralHealth As New BooleanField
            <DataMember()>
            Public Property Referrals_EarlyIntervention As New BooleanField
            <DataMember()>
            Public Property Referrals_MentalHealthSpecialist As New BooleanField
            <DataMember()>
            Public Property Referrals_Nutritionist As New BooleanField
            <DataMember()>
            Public Property Referrals_Optometrist As New BooleanField
            <DataMember()>
            Public Property Referrals_Ophthalmology As New BooleanField
            <DataMember()>
            Public Property Referrals_OtherSpecialtyText As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)

            <DataMember()>
            Public Property Immuno_Others_Checkbox As New BooleanField
            <DataMember()>
            Public Property Labs_Others_Checkbox As New BooleanField
            <DataMember()>
            Public Property Referals_Others_Checkbox As New BooleanField

            <DataMember()>
            Public Property Lab_HgbHct_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_TB_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_UA_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_LipidProfile_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_BloodLeadTest_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_VIH_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_NAAT_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_VDRL_Ordered As New BooleanField
            <DataMember()>
            Public Property Lab_Other_Ordered As New BooleanField

            <DataMember()>
            Public Property Lab_HgbHct_Result As New StringField
            <DataMember()>
            Public Property Lab_TB_Result As New StringField
            <DataMember()>
            Public Property Lab_UA_Result As New StringField
            <DataMember()>
            Public Property Lab_LipidProfile_Result As New StringField
            <DataMember()>
            Public Property Lab_BloodLeadTest_Result As New StringField
            <DataMember()>
            Public Property Lab_VIH_Result As New StringField
            <DataMember()>
            Public Property Lab_NAAT_Result As New StringField
            <DataMember()>
            Public Property Lab_VDRL_Result As New StringField
            <DataMember()>
            Public Property Lab_Other_Result As New StringField
        End Class

#End Region

#Region " Gastrointestinal PT "

        <Serializable(), DataContract()>
        Public Class GastrointestinalSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "GI"
                Me.NA = New BooleanField() With {.Value = False}
                Me.NASH = New BooleanField() With {.Value = False}
                Me.MetabolicSyndrome = New BooleanField() With {.Value = False}
                Me.Hyperkalemia = New BooleanField() With {.Value = False}
                Me.Hypokalemia = New BooleanField() With {.Value = False}
            End Sub

            Private _NA As New BooleanField

            <DataMember()>
            Public Property NA() As BooleanField
                Get
                    Return _NA
                End Get
                Set(ByVal value As BooleanField)
                    _NA = value
                End Set
            End Property

            Private _NASH As New BooleanField
            <DataMember()>
            Public Property NASH() As BooleanField
                Get
                    Return _NASH
                End Get
                Set(ByVal value As BooleanField)
                    _NASH = value
                End Set
            End Property

            Private _MetabolicSyndrome As New BooleanField
            <DataMember()>
            Public Property MetabolicSyndrome() As BooleanField
                Get
                    Return _MetabolicSyndrome
                End Get
                Set(ByVal value As BooleanField)
                    _MetabolicSyndrome = value
                End Set
            End Property

            Private _Hyperkalemia As New BooleanField
            <DataMember()>
            Public Property Hyperkalemia() As BooleanField
                Get
                    Return _Hyperkalemia
                End Get
                Set(ByVal value As BooleanField)
                    _Hyperkalemia = value
                End Set
            End Property

            Private _Hypokalemia As New BooleanField
            <DataMember()>
            Public Property Hypokalemia() As BooleanField
                Get
                    Return _Hypokalemia
                End Get
                Set(ByVal value As BooleanField)
                    _Hypokalemia = value
                End Set
            End Property

            <DataMember()>
            Public Property GastrointestinalTreatmentPlan As New StringField

        End Class

#End Region

#Region " Musculoskeletal "

        <Serializable(), DataContract()>
        Public Class MusculoskeletalSection
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "MK"
                Me.NA = New BooleanField() With {.Value = False}
                Me.Spondylosis = New BooleanField() With {.Value = False}
                Me.CervicalDiscDisorder = New BooleanField() With {.Value = False}
                Me.CervicothoracicRadiculopathy = New BooleanField() With {.Value = False}
                Me.CervicalRegion = New BooleanField() With {.Value = False}
                Me.CervicothoracicRegion = New BooleanField() With {.Value = False}
            End Sub

            Private _NA As New BooleanField

            <DataMember()>
            Public Property NA() As BooleanField
                Get
                    Return _NA
                End Get
                Set(ByVal value As BooleanField)
                    _NA = value
                End Set
            End Property

            Private _Spondylosis As New BooleanField
            <DataMember()>
            Public Property Spondylosis() As BooleanField
                Get
                    Return _Spondylosis
                End Get
                Set(ByVal value As BooleanField)
                    _Spondylosis = value
                End Set
            End Property

            Private _CervicalDiscDisorder As New BooleanField
            <DataMember()>
            Public Property CervicalDiscDisorder() As BooleanField
                Get
                    Return _CervicalDiscDisorder
                End Get
                Set(ByVal value As BooleanField)
                    _CervicalDiscDisorder = value
                End Set
            End Property

            Private _CervicothoracicRadiculopathy As New BooleanField
            <DataMember()>
            Public Property CervicothoracicRadiculopathy() As BooleanField
                Get
                    Return _CervicothoracicRadiculopathy
                End Get
                Set(ByVal value As BooleanField)
                    _CervicothoracicRadiculopathy = value
                End Set
            End Property

            Private _CervicalRegion As New BooleanField
            <DataMember()>
            Public Property CervicalRegion() As BooleanField
                Get
                    Return _CervicalRegion
                End Get
                Set(ByVal value As BooleanField)
                    _CervicalRegion = value
                End Set
            End Property

            Private _CervicothoracicRegion As New BooleanField
            <DataMember()>
            Public Property CervicothoracicRegion() As BooleanField
                Get
                    Return _CervicothoracicRegion
                End Get
                Set(ByVal value As BooleanField)
                    _CervicothoracicRegion = value
                End Set
            End Property

            <DataMember()>
            Public Property MusculoskeletalTreatmentPlan As New StringField

        End Class

#End Region

        <Serializable(), DataContract()>
        Public Class AHADxHxSelection

            Public Sub New()
                Me.ID = 0
                Me.ClaimID = 0
                Me.DxCode = String.Empty
                Me.DxDescription = String.Empty
                Me.ProviderName = String.Empty
                Me.Source = String.Empty
                Me.SelectionIndex = 0
                Me.ReasonForNo = 0
            End Sub

            <DataMember()>
            Public Property ID As Long
            <DataMember()>
            Public Property ClaimID As Long
            <DataMember()>
            Public Property DxCode As String
            <DataMember()>
            Public Property DxDescription As String
            <DataMember()>
            Public Property ProviderName As String
            <DataMember()>
            Public Property Source As String
            <DataMember()>
            Public Property SelectionIndex As Integer ' 0 = NO, 1 = YES, 2 = Maybe
            <DataMember()>
            Public Property ReasonForNo As Short
        End Class

#Region " Response Classes"
        <Serializable(), DataContract()>
        Public Class AHASubmissionResponse
            Inherits GenericResponse

            Private _AHAForm As AHAFormItem

            <DataMember()>
            Public Property AHAForm() As AHAFormItem
                Get
                    Return _AHAForm
                End Get
                Set(ByVal value As AHAFormItem)
                    _AHAForm = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class GetAHAItemResponse
            Inherits AHASubmissionResponse

            Private _AHAItem As New AHAForm2.AHAFormItem

            <DataMember()>
            Public Property AHAItem() As AHAForm2.AHAFormItem
                Get
                    Return _AHAItem
                End Get
                Set(ByVal value As AHAForm2.AHAFormItem)
                    _AHAItem = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class AHASubmissionShortResponse
            Inherits GenericResponse

            Public Sub New()
                Me.AHAForm = New AHAFormItemShort()
            End Sub

            Private _AHAForm As AHAFormItemShort
            <DataMember()>
            Public Property AHAForm() As AHAFormItemShort
                Get
                    Return _AHAForm
                End Get
                Set(ByVal value As AHAFormItemShort)
                    _AHAForm = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class GetAHAItemShortResponse
            Inherits AHASubmissionResponse

            Private _AHAItem As New AHAForm2.AHAFormItemShort
            Private _AHAFormShort As New AHAFormItemShort

            <DataMember()>
            Public Property AHAItem() As AHAForm2.AHAFormItemShort
                Get
                    Return _AHAItem
                End Get
                Set(ByVal value As AHAForm2.AHAFormItemShort)
                    _AHAItem = value
                End Set
            End Property

            <DataMember()>
            Public Property AHAFormShort() As AHAFormItemShort
                Get
                    Return _AHAFormShort
                End Get
                Set(ByVal value As AHAFormItemShort)
                    _AHAFormShort = value
                End Set
            End Property
        End Class

        <Serializable(), DataContract()>
        Public Class GetMemberConditionResponse
            Inherits GenericResponse

            Private _Conditions As List(Of Condition)
            <DataMember()>
            Public Property Conditions() As List(Of Condition)
                Get
                    Return _Conditions
                End Get
                Set(ByVal value As List(Of Condition))
                    _Conditions = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class GetProviderStatusLetterCSVResponse
            Inherits GenericResponse

            Private _csvBytes() As Byte
            <DataMember()>
            Public Property CSVData() As Byte()
                Get
                    Return _csvBytes
                End Get
                Set(ByVal value As Byte())
                    _csvBytes = value
                End Set
            End Property

            <DataMember()>
            Public Property FileName As String

        End Class

        <Serializable(), DataContract()>
        Public Class GetAHAReportBytesResponse
            Inherits GenericResponse

            Private _reportBytes As Byte()
            <DataMember()>
            Public Property ReportBytes() As Byte()
                Get
                    Return _reportBytes
                End Get
                Set(ByVal value As Byte())
                    _reportBytes = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class GetAddendumFormReportBytesResponse
            Inherits GenericResponse

            Private _reportBytes As Byte()
            Public Property ReportBytes() As Byte()
                Get
                    Return _reportBytes
                End Get
                Set(ByVal value As Byte())
                    _reportBytes = value
                End Set
            End Property

        End Class

        <Serializable(), DataContract()>
        Public Class CheckOrphanMemberHasAHAResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property HasAHA As Boolean

        End Class


        <Serializable(), DataContract()>
        Public Class GetProviderInformationResponse
            Inherits GenericResponse

            Public Sub New()
                ProviderInfoList = New List(Of ProviderItem)
            End Sub

            <DataMember()>
            Public Property ProviderInfoList As List(Of ProviderItem)

        End Class

        <Serializable(), DataContract()>
        Public Class GetPendingClaimResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property ClaimID As Long

        End Class

        <Serializable(), DataContract()>
        Public Class MemberItem
            <DataMember()>
            Public Property MemberID() As String
            <DataMember()>
            Public Property PayerID() As String
            <DataMember()>
            Public Property BillingNPI() As String
            <DataMember()>
            Public Property RenderingNPI() As String
            <DataMember()>
            Public Property Name() As String
            <DataMember()>
            Public Property ProviderName() As String
            <DataMember()>
            Public Property DOB() As Date
            <DataMember()>
            Public Property Gender() As String
            <DataMember()>
            Public Property Cover() As String
            <DataMember()>
            Public Property FirstName As String
            <DataMember()>
            Public Property MiddleName As String
            <DataMember()>
            Public Property LastName As String
        End Class

        <Serializable(), DataContract()>
        Public Class MemberEligibilityResponse
            Inherits GenericResponse

            Public Sub New()

            End Sub

            <DataMember()>
            Public Property MemberInfo As New MemberItem

        End Class

        <Serializable(), DataContract()>
        Public Class GetRejectNotesResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property RejectNotes As String

        End Class

        <Serializable(), DataContract()>
        Public Class SaveAHADxHxSelectionResponse
            Inherits GenericResponse

        End Class

        <Serializable(), DataContract()>
        Public Class SaveSuspiciousCondDxHxSelectionResponse
            Inherits GenericResponse

        End Class

        <Serializable(), DataContract()>
        Public Class GetAHADxHxSelectionResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property AHADxHxSelectionList As New List(Of AHADxHxSelection)

        End Class

        <Serializable(), DataContract()>
        Public Class GetMemberAHADxHistoryResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property AHADxHxSelectionList As New List(Of AHADxHxSelection)

        End Class

        <Serializable(), DataContract()>
        Public Class AddendeumProviderSaveResponse
            Inherits GenericResponse

        End Class

        <Serializable(), DataContract()>
        Public Class GetAHAHeaderResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property HeaderList As New List(Of FormHeaderSection)

        End Class

        <Serializable(), DataContract()>
        Public Class GetAHAYearItemResponse
            Inherits GenericResponse

            <DataMember()>
            Public Property Year As Integer
            <DataMember()>
            Public Property ClaimClass As Integer
            <DataMember()>
            Public Property IsReadOnly As Boolean
            <DataMember()>
            Public Property ReadOnlyDate As Nullable(Of DateTime)
            <DataMember()>
            Public Property ReadOnlyBy As String

        End Class

#End Region

        <Serializable(), DataContract()>
        Public Class Condition

            Private _Index As Long
            <DataMember()>
            Public Property Index() As Long
                Get
                    Return _Index
                End Get
                Set(ByVal value As Long)
                    _Index = value
                End Set
            End Property

            <DataMember()>
            Public Property Condition As String
            <DataMember()>
            Public Property Detail As String
            <DataMember()>
            Public Property DxCode As String
            <DataMember()>
            Public Property ClaimId As Long
            <DataMember()>
            Public Property SelectionIndex As Integer
            <DataMember()>
            Public Property HCC As String
            <DataMember()>
            Public Property Source As String
            <DataMember()>
            Public Property Type As String
            <DataMember()>
            Public Property ReasonForNo As Short
        End Class

        <Serializable(), DataContract()>
        Public Class SummaryCount
            <DataMember()>
            Public Property iStatus As Char
            <DataMember()>
            Public Property nClaimClass As Short
            <DataMember()>
            Public Property AtHome As Boolean
            <DataMember()>
            Public Property PayerID As String
            <DataMember()>
            Public Property Amount As Short
            <DataMember()>
            Public Property FormType As String

            Public Sub New()
                Amount = 0
            End Sub
        End Class

#Region " PHQ9"

        <Serializable(), DataContract()>
        Public Class PHQ9
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.Q1 = New IntegerField()
                Me.Q2 = New IntegerField()
                Me.Q3 = New IntegerField()
                Me.Q4 = New IntegerField()
                Me.Q5 = New IntegerField()
                Me.Q6 = New IntegerField()
                Me.Q7 = New IntegerField()
                Me.Q8 = New IntegerField()
                Me.Q9 = New IntegerField()
                Me.TotalCol1 = New IntegerField()
                Me.TotalCol2 = New IntegerField()
                Me.TotalCol3 = New IntegerField()
                Me.AllTotals = New IntegerField()
                Me.NotDifficultAtAll = New BooleanField()
                Me.SomewhatDifficult = New BooleanField()
                Me.VeryDifficult = New BooleanField()
                Me.ExtremelyDifficult = New BooleanField()
                Me.DepressedPastYear = New BooleanField()
                Me.SuicidePastMonth = New BooleanField()
                Me.TriedSuicide = New BooleanField()
            End Sub

            <DataMember()>
            Public Property Q1 As New IntegerField
            <DataMember()>
            Public Property Q2 As New IntegerField
            <DataMember()>
            Public Property Q3 As New IntegerField
            <DataMember()>
            Public Property Q4 As New IntegerField
            <DataMember()>
            Public Property Q5 As New IntegerField
            <DataMember()>
            Public Property Q6 As New IntegerField
            <DataMember()>
            Public Property Q7 As New IntegerField
            <DataMember()>
            Public Property Q8 As New IntegerField
            <DataMember()>
            Public Property Q9 As New IntegerField
            <DataMember()>
            Public Property TotalCol1 As New IntegerField
            <DataMember()>
            Public Property TotalCol2 As New IntegerField
            <DataMember()>
            Public Property TotalCol3 As New IntegerField
            <DataMember()>
            Public Property AllTotals As New IntegerField
            <DataMember()>
            Public Property NotDifficultAtAll As New BooleanField
            <DataMember()>
            Public Property SomewhatDifficult As New BooleanField
            <DataMember()>
            Public Property VeryDifficult As New BooleanField
            <DataMember()>
            Public Property ExtremelyDifficult As New BooleanField

            <DataMember()>
            Public Property DepressedPastYear As New BooleanField
            <DataMember()>
            Public Property SuicidePastMonth As New BooleanField
            <DataMember()>
            Public Property TriedSuicide As New BooleanField

        End Class

#End Region

#Region " Social Determinants"

        <Serializable(), DataContract()>
        Public Class SocialDetermiants
            Inherits AHASectionErrorSummary

            Public Sub New()
            End Sub

            <DataMember()>
            Public Property CurrentlyHasPlaceToLive As BooleanField
            <DataMember()>
            Public Property CurrentlyHasPlaceToLiveComments As StringField
            <DataMember()>
            Public Property InPast12MBeenAfraidOutOfFood As BooleanField
            <DataMember()>
            Public Property InPast12MBeenAfraidOutOfFoodComments As StringField
            <DataMember()>
            Public Property InPast12MLackOfTransportation As BooleanField
            <DataMember()>
            Public Property InPast12MLackOfTransportationComments As StringField
            <DataMember()>
            Public Property InPast12MBeenRiskCutOffWaterElectricity As BooleanField
            <DataMember()>
            Public Property InPast12MBeenRiskCutOffWaterElectricityComments As StringField
            <DataMember()>
            Public Property HaveProblemFindingCareForChild As BooleanField
            <DataMember()>
            Public Property HaveProblemFindingCareForChildComments As StringField
            <DataMember()>
            Public Property HaveJob As BooleanField
            <DataMember()>
            Public Property HaveJobComments As StringField
            <DataMember()>
            Public Property HaveHighSchoolDegree As BooleanField
            <DataMember()>
            Public Property HaveHighSchoolDegreeComments As StringField
            <DataMember()>
            Public Property HaveBeenSituationNoMoneyForBills As BooleanField
            <DataMember()>
            Public Property HaveBeenSituationNoMoneyForBillsComments As StringField
            <DataMember()>
            Public Property SomeoneHurtYou As BooleanField
            <DataMember()>
            Public Property SomeoneHurtYouComments As StringField

        End Class

#End Region

#Region " History Present Illness Options"

        <Serializable(), DataContract()>
        Public Class HistoryPresentIllnessOptions

            <DataMember()>
            Public Property ID As Integer
            <DataMember()>
            Public Property TextToShow As String
            <DataMember()>
            Public Property TextToShowEn As String

        End Class

#End Region


#Region "GastrointestinalDiseases"
        <Serializable(), DataContract()>
        Public Class GastrointestinalDiseases
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "GID"
            End Sub

            <DataMember()>
            Public Property NA As New BooleanField

            <DataMember()>
            Public Property NonalcoholicSteatohepatitis As New BooleanField

            <DataMember()>
            Public Property MetabolicSyndrome As New BooleanField

            <DataMember()>
            Public Property Hyperkalemia As New BooleanField

            <DataMember()>
            Public Property Hypokalemia As New BooleanField

            <DataMember()>
            Public Property LiverTransplant As New BooleanField


            <DataMember()>
            Public Property GERD As New BooleanField


            <DataMember()>
            Public Property ChronicHepatitis As New BooleanField


            <DataMember()>
            Public Property DiverticularDisease As New BooleanField


            <DataMember()>
            Public Property PepticUlcerDisease As New BooleanField

            <DataMember()>
            Public Property GastrointestinalTreatmentPlan As New StringField

        End Class

#End Region


#Region " Pulmonary Diseases"

        <Serializable(), DataContract()>
        Public Class PulmonaryDiseases
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "PD"
            End Sub

            <DataMember()>
            Public Property NA As New BooleanField

            <DataMember()>
            Public Property Asthma As New BooleanField
            <DataMember()>
            Public Property AsthmaComments As New StringField
            <DataMember()>
            Public Property AsthmaDescription As New StringField
            <DataMember()>
            Public Property AcuteBronchitis As New BooleanField
            <DataMember()>
            Public Property AcuteBronchitisComments As New StringField
            <DataMember()>
            Public Property ChronicBronchitis As New BooleanField
            <DataMember()>
            Public Property ChronicBronchitisComments As New StringField
            <DataMember()>
            Public Property COPD As New BooleanField
            <DataMember()>
            Public Property COPDComments As New StringField
            <DataMember()>
            Public Property PulmonaryFibrosis As New BooleanField
            <DataMember()>
            Public Property PulmonaryFibrosisComments As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)
            <DataMember()>
            Public Property UpperRespiratoryTractInfection As New BooleanField
            <DataMember()>
            Public Property AcuteLaryngopharyngitis As New BooleanField
            <DataMember()>
            Public Property AcuteNasopharyngitis As New BooleanField
            <DataMember()>
            Public Property UpperRespiratoryTractInfectionComments As New StringField
            <DataMember()>
            Public Property AcuteLaryngopharyngitisComments As New StringField
            <DataMember()>
            Public Property AcuteNasopharyngitisComments As New StringField
            <DataMember()>
            Public Property PulmonaryDiseases_OtherCondition As New StringField
            <DataMember()>
            Public Property PulmonaryDiseases_OtherConditionTreatment As New StringField
            <DataMember()>
            Public Property PulmonaryDiseases_OtherCondition_Checkbox As New BooleanField


            <DataMember()>
            Public Property LungTransplant As New BooleanField
            <DataMember()>
            Public Property LungTransplantTreatmentPlan As New StringField

        End Class

#End Region

#Region " Congenital Disease"

        <Serializable(), DataContract()>
        Public Class CongenitalDiseases
            Inherits AHASectionErrorSummary

            Public Sub New()
                Me.SectionID = "CONGD"
                Me.CongenitalDiseases_NA = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_SpinaBifida = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_SpinaBifidaComments = New StringField()
                Me.CongenitalDiseases_Hydrocephalus = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_HydrocephalusComments = New StringField()
                Me.CongenitalDiseases_ChiariMalformation = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_ChiariMalformationComments = New StringField()
                Me.CongenitalDiseases_Hemophilia = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_HemophiliaComments = New StringField()
                Me.CongenitalDiseases_Cranofacial = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_CranofacialComments = New StringField()
                Me.CongenitalDiseases_DistrofiaMuscular = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_DistrofiaMuscularComments = New StringField()
                Me.CongenitalDiseases_CerebralPalsy = New BooleanField() With {.Value = False}
                Me.CongenitalDiseases_CerebralPalsyText = New StringField()
                Me.CongenitalDiseases_CerebralPalsyComments = New StringField()
                Me.MedicationList = New List(Of String)()
            End Sub

            <DataMember()>
            Public Property CongenitalDiseases_NA As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_SpinaBifida As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_SpinaBifidaComments As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_Hydrocephalus As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_HydrocephalusComments As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_ChiariMalformation As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_ChiariMalformationComments As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_Hemophilia As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_HemophiliaComments As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_Cranofacial As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_CranofacialComments As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_DistrofiaMuscular As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_DistrofiaMuscularComments As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_CerebralPalsy As New BooleanField
            <DataMember()>
            Public Property CongenitalDiseases_CerebralPalsyText As New StringField
            <DataMember()>
            Public Property CongenitalDiseases_CerebralPalsyComments As New StringField
            <DataMember()>
            Public Property MedicationList As New List(Of String)

        End Class

#End Region

#Region " MalnutritionCriteria"

        <Serializable(), DataContract()>
        Public Class MalnutritionCriteria
            Inherits AHASectionErrorSummary

            Public Sub New()

            End Sub

            <DataMember()>
            Public Property involuntary_weight_loss As New BooleanField
            <DataMember()>
            Public Property involuntary_weight_lossmore10at6monthandmore20over6month As New BooleanField
            <DataMember()>
            Public Property involuntary_weight_loss10to5at6monthand20to10over6month As New BooleanField
            <DataMember()>
            Public Property involuntary_weight_lossless5at6monthandless10over6month As New BooleanField
            <DataMember()>
            Public Property Low_bmi As New BooleanField
            <DataMember()>
            Public Property low_bmiless18 As New BooleanField
            <DataMember()>
            Public Property low_bmiless20 As New BooleanField
            <DataMember()>
            Public Property reduced_muscle As New BooleanField
            <DataMember()>
            Public Property reduced_muscle_severly As New BooleanField
            <DataMember()>
            Public Property reduced_muscle_mild As New BooleanField
            <DataMember()>
            Public Property reduced_food_intake As New BooleanField
            <DataMember()>
            Public Property disease_burden As New BooleanField
            <DataMember()>
            Public Property malnutrition_criteria_total As New IntegerField
            <DataMember()>
            Public Property Malnutrition_Criteria_Result As New StringField
            <DataMember()>
            Public Property biClaimID As New StringField

            <DataMember()>
            Public Property other_criteria As New BooleanField

            <DataMember()>
            Public Property other_criteria_description As New StringField

            <DataMember()>
            Public Property albumin As New BooleanField

            <DataMember()>
            Public Property less2albumin As New BooleanField

            <DataMember()>
            Public Property less25albumin As New BooleanField

            <DataMember()>
            Public Property less35albumin As New BooleanField



        End Class

#End Region

#Region " ScreeningSubstanceUse"

        <Serializable(), DataContract()>
        Public Class ScreeningSubstanceUse
            Inherits AHASectionErrorSummary

            Public Sub New()

            End Sub

            <DataMember()>
            Public Property criteria_id As New IntegerField
            <DataMember()>
            Public Property screening_substance_use_q1 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q2 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q3 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q4 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q5 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q6 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q7 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q8 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q9 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q10 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q11 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q12 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_q13 As New BooleanField
            <DataMember()>
            Public Property screening_substance_use_total As New IntegerField
            <DataMember()>
            Public Property screening_substance_use_other As New StringField
            <DataMember()>
            Public Property Screening_Substance_Result As New StringField

        End Class

#End Region

#Region "SocialDeterminants"

        Public Class SocialDeterminants
            Inherits AHASectionErrorSummary

            <DataMember()>
            Public Property problems_living_alone As New BooleanField

            <DataMember()>
            Public Property illiteracy As New BooleanField

            <DataMember()>
            Public Property homelessness As New BooleanField

            <DataMember()>
            Public Property inadequate_home As New BooleanField

            <DataMember()>
            Public Property discord_with_nll As New BooleanField

            <DataMember()>
            Public Property problems_residential_institution As New BooleanField

            <DataMember()>
            Public Property lack_of_food_and_water As New BooleanField

            <DataMember()>
            Public Property extreme_poverty As New BooleanField

            <DataMember()>
            Public Property worried_about_losing_housing As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_rx As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_utilities As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_medical_care As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_phone As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_transportation As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_clothing As New BooleanField

            <DataMember()>
            Public Property problems_in_relationship As New BooleanField

            <DataMember()>
            Public Property absence_family_member_military As New BooleanField

            <DataMember()>
            Public Property other_absence_family_member As New BooleanField

            <DataMember()>
            Public Property disappearance_family_member As New BooleanField

            <DataMember()>
            Public Property disruption_separation As New BooleanField

            <DataMember()>
            Public Property dependent_at_home As New BooleanField

            <DataMember()>
            Public Property alcoholism_drug_addiction_family As New BooleanField

            <DataMember()>
            Public Property innapropriate_diet As New BooleanField

            <DataMember()>
            Public Property other_reduced_mobility As New BooleanField

            <DataMember()>
            Public Property need_personal_care As New BooleanField

            <DataMember()>
            Public Property need_at_home As New BooleanField

            <DataMember()>
            Public Property need_continuous_supervision As New BooleanField

            <DataMember()>
            Public Property other_problems_provider_dependency As New BooleanField

            <DataMember()>
            Public Property unavailability_other_helping_agencies As New BooleanField

            <DataMember()>
            Public Property partialy_depends_no_resource As New BooleanField

            <DataMember()>
            Public Property bedridden_few_to_no_resources As New BooleanField

            <DataMember()>
            Public Property need_assisstance_daily_activities As New BooleanField
        End Class

#End Region

#Region "SocialDeterminants2020"

        Public Class SocialDeterminants2020
            Inherits AHASectionErrorSummary
            <DataMember()>
            Public Property criteria_id As New IntegerField

            <DataMember()>
            Public Property problems_living_alone As New BooleanField


            <DataMember()>
            Public Property illiteracy As New BooleanField

            <DataMember()>
            Public Property homelessness As New BooleanField

            <DataMember()>
            Public Property inadequate_home As New BooleanField

            <DataMember()>
            Public Property discord_with_nll As New BooleanField

            <DataMember()>
            Public Property problems_residential_institution As New BooleanField

            <DataMember()>
            Public Property lack_of_food_and_water As New BooleanField

            <DataMember()>
            Public Property extreme_poverty As New BooleanField

            <DataMember()>
            Public Property worried_about_losing_housing As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_rx As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_utilities As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_medical_care As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_phone As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_transportation As New BooleanField

            <DataMember()>
            Public Property not_able_to_pay_clothing As New BooleanField

            <DataMember()>
            Public Property problems_in_relationship As New BooleanField

            <DataMember()>
            Public Property absence_family_member_military As New BooleanField

            <DataMember()>
            Public Property other_absence_family_member As New BooleanField

            <DataMember()>
            Public Property disappearance_family_member As New BooleanField

            <DataMember()>
            Public Property disruption_separation As New BooleanField

            <DataMember()>
            Public Property dependent_at_home As New BooleanField

            <DataMember()>
            Public Property alcoholism_drug_addiction_family As New BooleanField

            <DataMember()>
            Public Property innapropriate_diet As New BooleanField

            <DataMember()>
            Public Property other_reduced_mobility As New BooleanField

            <DataMember()>
            Public Property need_personal_care As New BooleanField

            <DataMember()>
            Public Property need_at_home As New BooleanField

            <DataMember()>
            Public Property need_continuous_supervision As New BooleanField

            <DataMember()>
            Public Property other_problems_provider_dependency As New BooleanField

            <DataMember()>
            Public Property unavailability_other_helping_agencies As New BooleanField

            <DataMember()>
            Public Property Social_Determinants_Result As New StringField

            <DataMember()>
            Public Property biClaimID As New StringField

            <DataMember()>
            Public Property partialy_depends_no_resource As New BooleanField

            <DataMember()>
            Public Property bedridden_few_to_no_resources As New BooleanField

            <DataMember()>
            Public Property need_assisstance_daily_activities As New BooleanField




        End Class

#End Region

#Region "SocialDeterminants2023"
        Public Class SocialDeterminants2023
            Inherits AHASectionErrorSummary
            <DataMember()>
            Public Property criteria_id As New IntegerField
            <DataMember()>
            Public Property IsAutosufficientInRequestForTransport As New IntegerField
            <DataMember()>
            Public Property HasSafeRoof As New IntegerField
            <DataMember()>
            Public Property HasSufficientFundsForFood As New IntegerField
            <DataMember()>
            Public Property FeelSafeInLivingPlace As New IntegerField
            <DataMember()>
            Public Property Social_Determinants_Result As New StringField

            Public Function IsNotFilled() As Boolean
                If HasSafeRoof.Value Is Nothing OrElse IsAutosufficientInRequestForTransport.Value Is Nothing OrElse
                   HasSufficientFundsForFood.Value Is Nothing Then
                    Return True
                Else
                    Return False
                End If
            End Function
        End Class
#End Region

#Region "AHASmartAI"

        <Serializable(), DataContract()>
        Public Class AHAAIInfo
            Private _ClaimID As Long
            Private _ContractID As Long
            Private _AudioBytes As Byte()
            Private _Transcript As String
            Private _HasFillAHA As Boolean

            <DataMember()>
            Public Property ClaimID() As Long
                Get
                    Return _ClaimID
                End Get
                Set(ByVal value As Long)
                    _ClaimID = value
                End Set
            End Property

            <DataMember()>
            Public Property ContractID() As Long
                Get
                    Return _ContractID
                End Get
                Set(ByVal value As Long)
                    _ContractID = value
                End Set
            End Property

            <DataMember()>
            Public Overridable Property AudioBytes() As Byte()
                Get
                    Return _AudioBytes
                End Get
                Set(ByVal value As Byte())
                    _AudioBytes = value
                End Set
            End Property

            <DataMember()>
            Public Overridable Property Transcript() As String
                Get
                    Return _Transcript
                End Get
                Set(ByVal value As String)
                    _Transcript = value
                End Set
            End Property

            <DataMember()>
            Public Overridable Property HasFillAHA() As Boolean
                Get
                    Return _HasFillAHA
                End Get
                Set(ByVal value As Boolean)
                    _HasFillAHA = value
                End Set
            End Property
        End Class

        <Serializable(), DataContract()>
        Public Class GetAHAAIInfoResponse
            Inherits GenericResponse

            Private _AudioBytes As Byte()
            Private _Transcript As String
            Private _HasFillAHA As Boolean

            <DataMember()>
            Public Overridable Property AudioBytes() As Byte()
                Get
                    Return _AudioBytes
                End Get
                Set(ByVal value As Byte())
                    _AudioBytes = value
                End Set
            End Property

            <DataMember()>
            Public Overridable Property Transcript() As String
                Get
                    Return _Transcript
                End Get
                Set(ByVal value As String)
                    _Transcript = value
                End Set
            End Property

            <DataMember()>
            Public Overridable Property HasFillAHA() As Boolean
                Get
                    Return _HasFillAHA
                End Get
                Set(ByVal value As Boolean)
                    _HasFillAHA = value
                End Set
            End Property

        End Class

#End Region

    End Namespace

End Namespace