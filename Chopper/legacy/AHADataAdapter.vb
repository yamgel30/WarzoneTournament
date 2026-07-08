Imports System.Data.DataTable
Imports System.Data.SqlClient
Imports AUS.Library.Data
Imports AHAController.AHAEDM 'AHAProvidersPortal.AHA2012
Imports AHAController.AHAEDM.AHAForm2
Imports System.Configuration.ConfigurationManager
Imports System.Reflection
Imports System.Text

Public Class AHADataAdapter

    Public Const ConfigConnKey As String = "AHAProviderPortal"
    Public Const ClaimAHADetailTableName As String = "Claims_AHADetail"

    Private _isGHP As Boolean
    Private _isAhaFL As Boolean
    Private _isAhaPR As Boolean
    Private _memberAge As Decimal
    Private _sessionID As Long
    Private _saveError As Boolean = False
    Private _saveErrorDetail As List(Of Dictionary(Of String, String))
    Private _ClaimClass As Integer = 144
    Public Sub New()

    End Sub

    '    'Please change to resemble Db values from M_ListItems
    '    Public Enum ListNames
    '        FamEnv = 1
    '        Frequency = 2
    '        ROS = 3
    '        Transportation = 4
    '    End Enum

    Private Function GetSqlObject() As SqlDataObject
        Return New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)
    End Function

    Private Function VerifyDateNull(ByVal nullableDate As Object) As Object

        If nullableDate IsNot Nothing Then
            If (CType(nullableDate, DateField).Value.HasValue) Then
                Return CType(nullableDate, DateField).Value.Value
            End If
        End If

        Return DBNull.Value

    End Function

    Private Function VerifyDateNull(ByVal nullableDate As Nullable(Of Date)) As Object

        If Not IsNothing(nullableDate) Then
            If (nullableDate.HasValue) Then
                Return nullableDate.Value
            End If
        End If

        Return DBNull.Value

    End Function

    Private Function VerifyBooleanNull(ByVal nullableBoolean As Nullable(Of Boolean)) As Object
        If Not IsNothing(nullableBoolean) Then
            If (nullableBoolean.HasValue) Then
                Return nullableBoolean.Value
            End If
        End If
        Return DBNull.Value
    End Function


    Private Function VerifyBooleanNull(ByVal nullableBoolean As Object) As Object

        If nullableBoolean IsNot Nothing Then
            If (CType(nullableBoolean, BooleanField).Value.HasValue) Then
                Return CType(nullableBoolean, BooleanField).Value.Value
            End If
        End If

        Return DBNull.Value

    End Function

    Private Function VerifyDecimalNull(ByVal nullableDecimal As Object) As Object

        If nullableDecimal IsNot Nothing Then
            If (CType(nullableDecimal, DecimalField).Value.HasValue) Then
                Return CType(nullableDecimal, DecimalField).Value
            End If
        End If

        Return DBNull.Value

    End Function

    Private Function VerifyIntegerNull(ByVal nullableInteger As Object) As Object

        If nullableInteger IsNot Nothing Then
            If (CType(nullableInteger, IntegerField).Value.HasValue) Then
                Return CType(nullableInteger, IntegerField).Value.Value
            End If
        End If

        Return DBNull.Value

    End Function

    Private Function VerifyNull(ByVal nullableShort As Nullable(Of Short)) As Object
        If (nullableShort.HasValue) Then
            Return nullableShort.Value
        End If
        Return DBNull.Value
    End Function

    Private Function VerifyStringNull(ByVal nullableString As StringField) As Object
        If Not IsNothing(nullableString) Then
            If Not IsNothing(nullableString.Value) Then
                Return nullableString.Value
            End If
        End If
        Return ""
    End Function

    Private Function VerifyStringNull(ByVal nullableString As Object) As Object
        If nullableString IsNot Nothing Then
            If (CType(nullableString, StringField).Value) IsNot Nothing Then
                Return CType(nullableString, StringField).Value.ToString.Trim
            End If
        End If
        Return ""
    End Function

    Public Function SaveClaim(ByVal token As String, ByVal sourceIp As String, ByVal isNew As Boolean, ByVal ahaItem As AHAFormItemShort, ByVal isResubmit As Boolean,
                            ByVal isPartialSave As Boolean, ByVal pageNum As Integer, Optional ByVal isAddendum As Boolean = False) As Boolean

        Dim ServiceCode = "G0438"
        'If _isGHP And _memberAge < 21 Then
        '    ServiceCode = ""
        'End If

        Dim errors As New List(Of String)
        Dim saveAHADetail As Boolean = False

        Dim dbo As SqlDataObject = GetSqlObject()
        Dim sqlCMD As SqlClient.SqlCommand
        Dim claimClass As Integer = _ClaimClass
        Dim claimTag As Integer = 4

        Try

            'Initialize claimClass

            'claimClass = 94 ' AppSettings("AHAClaimClassShort").ToString




            'Get SessionID by Token....

            'Dim sessionID As Long = 0
            'If Session("Sessions.biSessionID") IsNot Nothing AndAlso IsNumeric(Session("Sessions.biSessionID")) Then
            '    sessionID = Session("Sessions.biSessionID")
            'End If

            dbo.OpenConnection()

            sqlCMD = New SqlClient.SqlCommand
            sqlCMD.CommandType = CommandType.StoredProcedure

            Dim claimKeyReSubmitted As Long

            If isResubmit AndAlso ahaItem.ID.HasValue Then claimKeyReSubmitted = ahaItem.ID

            If Not isNew Then

                sqlCMD.CommandText = "uspClaims_MSVUpdt"

                sqlCMD.Parameters.AddWithValue("@biClaimID", ahaItem.ID.Value)
                sqlCMD.Parameters.AddWithValue("@sBillingNPI", ahaItem.FormHeader.BillingNPI.Value)
                sqlCMD.Parameters.AddWithValue("@sRenderingNPI", ahaItem.FormHeader.RenderingNPI.Value)
                sqlCMD.Parameters.AddWithValue("@dDOS", ahaItem.FormHeader.DateOfVisit.Value)
                sqlCMD.Parameters.AddWithValue("@nClaimClass", claimClass)
                sqlCMD.Parameters.AddWithValue("@dSubmitter", Date.Now)
                sqlCMD.Parameters.AddWithValue("@sSourceIP", sourceIp)
                sqlCMD.Parameters.AddWithValue("@nClaimClassTag", claimTag) 'AHA 2014 version 2, various changes (ROS, Active, Suspicious condition. Version 3 = AHA 2015 X 2 and AHA At Home
                sqlCMD.Parameters.AddWithValue("@biSessionID", ahaItem.SessionID)
                sqlCMD.Parameters.AddWithValue("@nPOS", ahaItem.FormHeader.PlaceOfService.Value.Value)
                sqlCMD.Parameters.AddWithValue("AHALanguage", ahaItem.Language)
                'sqlCMD.Parameters.AddWithValue("@Action", action)

                'Added in 2023
                If ahaItem.FormHeader.Race IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Race", ahaItem.FormHeader.Race.Value)
                If ahaItem.FormHeader.Ethnicity IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Ethnicity", ahaItem.FormHeader.Ethnicity.Value)
                If ahaItem.FormHeader.Phone IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Phone", ahaItem.FormHeader.Phone.Value)
                If ahaItem.FormHeader.Email IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Email", ahaItem.FormHeader.Email.Value)
                If ahaItem.FormHeader.AdditionalHealthPlan IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@AdditionalHealthPlan", ahaItem.FormHeader.AdditionalHealthPlan.Value)
                If ahaItem.FormHeader.AdditionalHealthPlanOther IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@AdditionalHealthPlanOther", ahaItem.FormHeader.AdditionalHealthPlanOther.Value)
                sqlCMD.Parameters.AddWithValue("Member_Lang", ahaItem.FormHeader.Member_Lang.Value)

                dbo.ExecuteCommand(sqlCMD)

            Else

                Dim Payers As Dictionary(Of String, Boolean) = Globals.ValidatePayerID(ahaItem)
                Dim _isGHP As Boolean = Payers.Item("isGHP")
                Dim _isAhaFL As Boolean = Payers.Item("isAhaFL")
                Dim _isAhaPR As Boolean = Payers.Item("isAhaPR")

                sqlCMD.CommandText = "uspClaims_Add"

                Dim claimICN As String = String.Format("{0}{1}{2}{3}", Now.Year.ToString,
                                                       Now.Month.ToString.PadLeft(2, "0"), Now.Day.ToString.PadLeft(2, "0"),
                                                       AppShared.GetIdentity("ClaimICN", dbo).ToString.PadLeft(8, "0"))

                sqlCMD.Parameters.AddWithValue("@biPayerSourceID", 0)
                sqlCMD.Parameters.AddWithValue("@sBarCode", CreateBarCode)
                sqlCMD.Parameters.AddWithValue("@sClaimICN", claimICN)
                sqlCMD.Parameters.AddWithValue("@sBillingNPI", ahaItem.FormHeader.BillingNPI.Value)
                sqlCMD.Parameters.AddWithValue("@sRenderingNPI", ahaItem.FormHeader.RenderingNPI.Value)
                'sqlCMD.Parameters.AddWithValue("@sPayerID", AppShared.GetMemberPayerID(AHAStep2.MemberID))
                sqlCMD.Parameters.AddWithValue("@sPayerID", ahaItem.FormHeader.PayerID.Value)
                sqlCMD.Parameters.AddWithValue("@sClaimNumber", claimICN)
                sqlCMD.Parameters.AddWithValue("@sPatientContract", ahaItem.FormHeader.MemberID.Value)
                sqlCMD.Parameters.AddWithValue("@sServiceCode", ServiceCode)
                sqlCMD.Parameters.AddWithValue("@dDOS", ahaItem.FormHeader.DateOfVisit.Value)

                If (_isGHP) Then
                    sqlCMD.Parameters.AddWithValue("@cCharges", 30)
                Else
                    sqlCMD.Parameters.AddWithValue("@cCharges", 150)
                End If


                sqlCMD.Parameters.AddWithValue("@dSubmitter", Date.Now)
                sqlCMD.Parameters.AddWithValue("@nClaimClass", claimClass)
                sqlCMD.Parameters.AddWithValue("@iStatus", IIf(Not isPartialSave, 0, (claimClass * -1)))
                sqlCMD.Parameters.AddWithValue("@sSourceIP", sourceIp)
                If ahaItem.FormHeader.ClaimClassTag IsNot Nothing AndAlso ahaItem.FormHeader.ClaimClassTag.Value = 4 Then
                    sqlCMD.Parameters.AddWithValue("@nClaimClassTag", ahaItem.FormHeader.ClaimClassTag.Value)
                Else
                    sqlCMD.Parameters.AddWithValue("@nClaimClassTag", 1)  'AHA 2014 version 2, various changes (ROS, Active, Suspicious condition. Version 3 = AHA 2015 X 2 and AHA At Home
                End If
                sqlCMD.Parameters.AddWithValue("@biSessionID", ahaItem.SessionID)
                sqlCMD.Parameters.AddWithValue("@nPOS", ahaItem.FormHeader.PlaceOfService.Value.Value)

                sqlCMD.Parameters.AddWithValue("sPatientFName", ahaItem.FormHeader.MemberFName)
                sqlCMD.Parameters.AddWithValue("sPatientLName", ahaItem.FormHeader.MemberLName)
                sqlCMD.Parameters.AddWithValue("PatientMName", ahaItem.FormHeader.MemberMName)

                sqlCMD.Parameters.AddWithValue("PatientBirth", ahaItem.FormHeader.MemberDOB.Value.Value)
                sqlCMD.Parameters.AddWithValue("RenderingName", ahaItem.FormHeader.ProviderName.Value)
                sqlCMD.Parameters.AddWithValue("MemberGender", ahaItem.FormHeader.MemberGender.Value)
                sqlCMD.Parameters.AddWithValue("Member_Lang", ahaItem.FormHeader.Member_Lang.Value)


                sqlCMD.Parameters.AddWithValue("BillingName", ahaItem.FormHeader.BillingName)
                sqlCMD.Parameters.AddWithValue("IPAName", ahaItem.FormHeader.IPAName)
                sqlCMD.Parameters.AddWithValue("AHALanguage", ahaItem.Language)

                ahaItem.ID = CLng(dbo.GetSingleValue(sqlCMD))

                If isNew AndAlso ahaItem.ID > 0 Then SaveAHADxHistSelection(ahaItem.ID, ahaItem.DxHistorySelectionList)

            End If


            Try
                saveAHADetail = SaveAHADetails(ahaItem, dbo, isPartialSave, pageNum)
            Catch ex As Exception
                Dim mp As New StringBuilder
                mp.AppendLine("SaveClaim, SaveAHADetail, ClaimID: " & ahaItem.ID.ToString())

                Try
                    Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
                Catch ex1 As Exception
                    mp.AppendLine("SaveClaim, SaveAHADetail. Error on Try Catch...")
                    If ex.StackTrace IsNot Nothing Then
                        mp.AppendLine("StactTrace...: " & ex1.StackTrace.ToString)
                    End If

                    Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex1.Message.ToString(), "")
                End Try

                SaveClaim = False

                Return SaveClaim

            End Try

            If saveAHADetail Then
                If Not isPartialSave Then
                    Try
                        If isResubmit Then
                            If Not isAddendum Then
                                AppShared.CloseClaim(ahaItem.ID, claimKeyReSubmitted, dbo)
                            End If
                        Else
                            AppShared.CloseClaim(ahaItem.ID, 0, dbo)
                        End If
                    Catch 'ex As Exception
                    End Try
                End If
            End If

            If saveAHADetail Then SaveClaim = True

        Catch 'ex As Exception
            SaveClaim = False
            'Throw
        Finally
            dbo.CloseConnection()
            dbo.Dispose()
        End Try

        Return SaveClaim

    End Function

    Public Function SaveClaim(ByVal token As String, ByVal sourceIp As String, ByVal isNew As Boolean, ByVal ahaItem As AHAFormItem, ByVal isResubmit As Boolean,
                            ByVal isPartialSave As Boolean, ByVal pageNum As Integer, Optional ByVal isAddendum As Boolean = False) As Boolean

        Dim ServiceCode = "G0438"
        'If _isGHP And _memberAge < 21 Then
        '    ServiceCode = ""
        'End If

        Dim errors As New List(Of String)
        Dim saveAHADetail As Boolean = False

        Dim dbo As SqlDataObject = GetSqlObject()
        Dim sqlCMD As SqlClient.SqlCommand
        Dim claimClass As Integer = claimClass

        Try

            'Initialize claimClass
            If AppSettings("AHAClaimClass") IsNot Nothing Then

                'Validate AppSetting has valid values, if not reset with default 24 AHA 2013 EDI
                Select Case AppSettings("AHAClaimClass").ToString
                    Case "36", "34", "46", "44", "54", "56", "64", "74", "84", "94", "104", "114", "124", "134", "144", "154"
                        claimClass = AppSettings("AHAClaimClass").ToString
                    Case Else
                        claimClass = 144
                End Select

            End If


            'Get SessionID by Token....

            'Dim sessionID As Long = 0
            'If Session("Sessions.biSessionID") IsNot Nothing AndAlso IsNumeric(Session("Sessions.biSessionID")) Then
            '    sessionID = Session("Sessions.biSessionID")
            'End If

            dbo.OpenConnection()

            sqlCMD = New SqlClient.SqlCommand
            sqlCMD.CommandType = CommandType.StoredProcedure

            Dim claimKeyReSubmitted As Long

            If isResubmit AndAlso ahaItem.ID.HasValue Then claimKeyReSubmitted = ahaItem.ID

            If Not isNew Then

                sqlCMD.CommandText = "uspClaims_MSVUpdt"


                sqlCMD.Parameters.AddWithValue("@biClaimID", ahaItem.ID.Value)
                sqlCMD.Parameters.AddWithValue("@sBillingNPI", ahaItem.FormHeader.BillingNPI.Value)
                sqlCMD.Parameters.AddWithValue("@sRenderingNPI", ahaItem.FormHeader.RenderingNPI.Value)
                sqlCMD.Parameters.AddWithValue("@dDOS", ahaItem.FormHeader.DateOfVisit.Value)
                sqlCMD.Parameters.AddWithValue("@nClaimClass", claimClass)
                sqlCMD.Parameters.AddWithValue("@dSubmitter", IIf(ahaItem.SubmittedDate.HasValue, ahaItem.SubmittedDate, Date.Now))
                sqlCMD.Parameters.AddWithValue("@sSourceIP", sourceIp)
                sqlCMD.Parameters.AddWithValue("@nClaimClassTag", 1) 'AHA 2014 version 2, various changes (ROS, Active, Suspicious condition. Version 3 = AHA 2015 X 2 and AHA At Home
                sqlCMD.Parameters.AddWithValue("@biSessionID", ahaItem.SessionID)
                sqlCMD.Parameters.AddWithValue("@nPOS", ahaItem.FormHeader.PlaceOfService.Value.Value)
                sqlCMD.Parameters.AddWithValue("AHALanguage", ahaItem.Language)

                If ahaItem.FormHeader.Member_Lang_other IsNot Nothing Then sqlCMD.Parameters.AddWithValue("Member_Lang", ahaItem.FormHeader.Member_Lang.Value)

                If ahaItem.FormHeader.Member_Lang_other IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_Language_other", ahaItem.FormHeader.Member_Lang_other.Value)


                'Added in 2023
                If ahaItem.FormHeader.Race IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Race", ahaItem.FormHeader.Race.Value)
                If ahaItem.FormHeader.Ethnicity IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Ethnicity", ahaItem.FormHeader.Ethnicity.Value)
                If ahaItem.FormHeader.Phone IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Phone", ahaItem.FormHeader.Phone.Value)
                If ahaItem.FormHeader.Email IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Email", ahaItem.FormHeader.Email.Value)
                If ahaItem.FormHeader.AdditionalHealthPlan IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@AdditionalHealthPlan", ahaItem.FormHeader.AdditionalHealthPlan.Value)
                If ahaItem.FormHeader.AdditionalHealthPlanOther IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@AdditionalHealthPlanOther", ahaItem.FormHeader.AdditionalHealthPlanOther.Value)

                If ahaItem.FormHeader.Sexual_Orientation IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Sexual_Orientation", ahaItem.FormHeader.Sexual_Orientation.Value)
                If ahaItem.FormHeader.Sex_Birth IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Sex_Birth", ahaItem.FormHeader.Sex_Birth.Value)
                If ahaItem.FormHeader.Pronoun IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Pronoun", ahaItem.FormHeader.Pronoun.Value)
                If ahaItem.FormHeader.Gender_Identity IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Gender_Identity", ahaItem.FormHeader.Gender_Identity.Value)


                If ahaItem.FormHeader.Sexual_OrientationSomethingelse IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_Sexual_OrientationSomethingelse", ahaItem.FormHeader.Sexual_OrientationSomethingelse.Value)
                If ahaItem.FormHeader.PronounOther_Pronoun IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_PronounOther_Pronoun", ahaItem.FormHeader.PronounOther_Pronoun.Value)
                If ahaItem.FormHeader.Other_Race IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_Other_Race", ahaItem.FormHeader.Other_Race.Value)
                If ahaItem.FormHeader.Gender_Identity_AdditionalGender IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_Gender_Identity_AdditionalGender", ahaItem.FormHeader.Gender_Identity_AdditionalGender.Value)


                'sqlCMD.Parameters.AddWithValue("@Action", action)

                dbo.ExecuteCommand(sqlCMD)

            Else

                Dim Payers As Dictionary(Of String, Boolean) = Globals.ValidatePayerID(ahaItem)
                Dim _isGHP As Boolean = Payers.Item("isGHP")
                Dim _isAhaFL As Boolean = Payers.Item("isAhaFL")
                Dim _isAhaPR As Boolean = Payers.Item("isAhaPR")

                sqlCMD.CommandText = "uspClaims_Add"

                Dim claimICN As String = String.Format("{0}{1}{2}{3}", Now.Year.ToString,
                                                       Now.Month.ToString.PadLeft(2, "0"), Now.Day.ToString.PadLeft(2, "0"),
                                                       AppShared.GetIdentity("ClaimICN", dbo).ToString.PadLeft(8, "0"))

                sqlCMD.Parameters.AddWithValue("@biPayerSourceID", 0)
                sqlCMD.Parameters.AddWithValue("@sBarCode", CreateBarCode)
                sqlCMD.Parameters.AddWithValue("@sClaimICN", claimICN)
                sqlCMD.Parameters.AddWithValue("@sBillingNPI", ahaItem.FormHeader.BillingNPI.Value)
                sqlCMD.Parameters.AddWithValue("@sRenderingNPI", ahaItem.FormHeader.RenderingNPI.Value)
                'sqlCMD.Parameters.AddWithValue("@sPayerID", AppShared.GetMemberPayerID(AHAStep2.MemberID))
                sqlCMD.Parameters.AddWithValue("@sPayerID", ahaItem.FormHeader.PayerID.Value)
                sqlCMD.Parameters.AddWithValue("@sClaimNumber", claimICN)
                sqlCMD.Parameters.AddWithValue("@sPatientContract", ahaItem.FormHeader.MemberID.Value)
                sqlCMD.Parameters.AddWithValue("@sServiceCode", ServiceCode)
                sqlCMD.Parameters.AddWithValue("@dDOS", ahaItem.FormHeader.DateOfVisit.Value)

                If (_isGHP) Then
                    sqlCMD.Parameters.AddWithValue("@cCharges", 30)
                Else
                    sqlCMD.Parameters.AddWithValue("@cCharges", 150)
                End If


                sqlCMD.Parameters.AddWithValue("@dSubmitter", Date.Now)
                sqlCMD.Parameters.AddWithValue("@nClaimClass", claimClass)
                sqlCMD.Parameters.AddWithValue("@iStatus", IIf(Not isPartialSave, 0, (claimClass * -1)))
                sqlCMD.Parameters.AddWithValue("@sSourceIP", sourceIp)
                sqlCMD.Parameters.AddWithValue("@nClaimClassTag", 1)  'AHA 2014 version 2, various changes (ROS, Active, Suspicious condition. Version 3 = AHA 2015 X 2 and AHA At Home
                sqlCMD.Parameters.AddWithValue("@biSessionID", ahaItem.SessionID)
                sqlCMD.Parameters.AddWithValue("@nPOS", ahaItem.FormHeader.PlaceOfService.Value.Value)

                sqlCMD.Parameters.AddWithValue("sPatientFName", ahaItem.FormHeader.MemberFName)
                sqlCMD.Parameters.AddWithValue("sPatientLName", ahaItem.FormHeader.MemberLName)
                sqlCMD.Parameters.AddWithValue("PatientMName", ahaItem.FormHeader.MemberMName)

                sqlCMD.Parameters.AddWithValue("PatientBirth", ahaItem.FormHeader.MemberDOB.Value.Value)
                sqlCMD.Parameters.AddWithValue("RenderingName", ahaItem.FormHeader.ProviderName.Value)
                sqlCMD.Parameters.AddWithValue("MemberGender", ahaItem.FormHeader.MemberGender.Value)

                sqlCMD.Parameters.AddWithValue("BillingName", ahaItem.FormHeader.BillingName)
                sqlCMD.Parameters.AddWithValue("IPAName", ahaItem.FormHeader.IPAName)
                sqlCMD.Parameters.AddWithValue("AHALanguage", ahaItem.Language)
                'Added in 2023

                If ahaItem.FormHeader.Race IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Race", ahaItem.FormHeader.Race.Value)
                If ahaItem.FormHeader.Ethnicity IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Ethnicity", ahaItem.FormHeader.Ethnicity.Value)
                If ahaItem.FormHeader.Phone IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Phone", ahaItem.FormHeader.Phone.Value)
                If ahaItem.FormHeader.Email IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Email", ahaItem.FormHeader.Email.Value)
                If ahaItem.FormHeader.AdditionalHealthPlan IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@AdditionalHealthPlan", ahaItem.FormHeader.AdditionalHealthPlan.Value)
                If ahaItem.FormHeader.AdditionalHealthPlanOther IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@AdditionalHealthPlanOther", ahaItem.FormHeader.AdditionalHealthPlanOther.Value)
                If ahaItem.FormHeader.Sexual_Orientation IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Sexual_Orientation", ahaItem.FormHeader.Sexual_Orientation.Value)
                If ahaItem.FormHeader.Sex_Birth IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Sex_Birth", ahaItem.FormHeader.Sex_Birth.Value)
                If ahaItem.FormHeader.Pronoun IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Pronoun", ahaItem.FormHeader.Pronoun.Value)
                If ahaItem.FormHeader.Gender_Identity IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Gender_Identity", ahaItem.FormHeader.Gender_Identity.Value)

                If ahaItem.FormHeader.Member_Lang IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_Lang", ahaItem.FormHeader.Member_Lang.Value)
                If ahaItem.FormHeader.Member_Lang_other IsNot Nothing Then sqlCMD.Parameters.AddWithValue("@Member_Language_other", ahaItem.FormHeader.Member_Lang_other.Value)


                ahaItem.ID = CLng(dbo.GetSingleValue(sqlCMD))

                If isNew AndAlso ahaItem.ID > 0 Then SaveAHADxHistSelection(ahaItem.ID, ahaItem.DxHistorySelectionList)
            End If

            Try
                saveAHADetail = SaveAHADetails(ahaItem, dbo, isPartialSave, pageNum)
            Catch ex As Exception
                Dim mp As New StringBuilder
                mp.AppendLine("SaveClaim, SaveAHADetail, ClaimID: " & ahaItem.ID.ToString())

                Try
                    Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
                Catch ex1 As Exception
                    mp.AppendLine("SaveClaim, SaveAHADetail. Error on Try Catch...")
                    If ex.StackTrace IsNot Nothing Then
                        mp.AppendLine("StactTrace...: " & ex1.StackTrace.ToString)
                    End If

                    Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex1.Message.ToString(), "")
                End Try

                SaveClaim = False

                Return SaveClaim

            End Try

            If saveAHADetail Then
                If Not isPartialSave Then
                    Try
                        If isResubmit Then
                            If Not isAddendum Then
                                AppShared.CloseClaim(ahaItem.ID, claimKeyReSubmitted, dbo)
                            End If
                        Else
                            AppShared.CloseClaim(ahaItem.ID, 0, dbo)
                        End If
                    Catch 'ex As Exception
                    End Try
                End If
            End If

            If saveAHADetail Then SaveClaim = True

        Catch ex As Exception
            SaveClaim = False
            'Throw
        Finally
            dbo.CloseConnection()
            dbo.Dispose()
        End Try

        Return SaveClaim

    End Function

    Private Function SaveAHADetails(ByVal ahaItem As AHAFormItem, ByVal dbo As SqlDataObject, ByVal isPartialSave As Boolean, ByVal pageNum As Integer) As Boolean
        Dim payers As Dictionary(Of String, Boolean) = Globals.ValidatePayerID(ahaItem)
        _isGHP = payers.Item("isGHP")
        _isAhaFL = payers.Item("isAhaFL")
        _isAhaPR = payers.Item("isAhaPR")
        _memberAge = TimeSpan.FromTicks(DateTime.Now.Ticks - ahaItem.FormHeader.MemberDOB.Value.Value.Ticks).TotalDays / 365.25
        _sessionID = ahaItem.SessionID
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 1) Then SavePage1(ahaItem, dbo)
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 2) Then SavePage2(ahaItem, dbo)
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 3) Then SavePage3(ahaItem, dbo)
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 4) Then SavePage4(ahaItem, dbo)
        Try
            If pageNum > 0 Then

                If pageNum = 1 Then SavePage1(ahaItem, dbo)
                If pageNum = 2 Then SavePage2(ahaItem, dbo)
                If pageNum = 3 Then SavePage3(ahaItem, dbo)
                If pageNum = 4 Then SavePage4(ahaItem, dbo)

            Else
                SavePage1(ahaItem, dbo)
                SavePage2(ahaItem, dbo)
                SavePage3(ahaItem, dbo)
                SavePage4(ahaItem, dbo)
            End If

            'Return True

            'If this function returns True on success, so if there is an error it must return the 
            'inverse of _saveError. This was the fastest solution given the time constraint.
            Return Not _saveError
        Catch ex As Exception
            Dim mp As New StringBuilder

            mp.AppendLine("Save AHA Detail, ClaimID: " & ahaItem.ID.ToString())

            Try
                Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
            Catch ex1 As Exception
                mp.AppendLine("Save AHA Detail. Error on Try Catch...")
                If ex.StackTrace IsNot Nothing Then
                    mp.AppendLine("StactTrace...: " & ex1.StackTrace.ToString)
                End If

                Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex1.Message.ToString(), "")
            End Try

            Return False
            'Throw

        End Try

    End Function

    Private Function SaveAHADetails(ByVal ahaItem As AHAFormItemShort, ByVal dbo As SqlDataObject, ByVal isPartialSave As Boolean, ByVal pageNum As Integer) As Boolean
        Dim payers As Dictionary(Of String, Boolean) = Globals.ValidatePayerID(ahaItem)
        _isGHP = payers.Item("isGHP")
        _isAhaFL = payers.Item("isAhaFL")
        _isAhaPR = payers.Item("isAhaPR")
        _memberAge = TimeSpan.FromTicks(DateTime.Now.Ticks - ahaItem.FormHeader.MemberDOB.Value.Value.Ticks).TotalDays / 365.25
        _sessionID = ahaItem.SessionID

        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 1) Then SavePage1(ahaItem, dbo)
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 2) Then SavePage2(ahaItem, dbo)
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 3) Then SavePage3(ahaItem, dbo)
        'If Not isPartialSave OrElse (isPartialSave AndAlso pageNum = 4) Then SavePage4(ahaItem, dbo)
        Try
            If pageNum > 0 Then

                If pageNum = 1 Then
                    SavePage1(ahaItem, dbo)
                    SaveShortForm(ahaItem.ID, ahaItem.formHeaderSeccion, ahaItem.covidSecction, dbo)
                End If
                If pageNum = 2 Then SavePage2(ahaItem, dbo)
                If pageNum = 3 Then SavePage3(ahaItem, dbo)
                If pageNum = 4 Then SavePage4(ahaItem, dbo)

            Else
                SavePage1(ahaItem, dbo)
                SaveShortForm(ahaItem.ID, ahaItem.FormHeader, ahaItem.covidSecction, dbo)
                SavePage2(ahaItem, dbo)
                SavePage3(ahaItem, dbo)
                SavePage4(ahaItem, dbo)


            End If

            'Return True

            'If this function returns True on success, so if there is an error it must return the 
            'inverse of _saveError. This was the fastest solution given the time constraint.
            Return Not _saveError
        Catch ex As Exception
            Dim mp As New StringBuilder

            mp.AppendLine("Save AHA Detail, ClaimID: " & ahaItem.ID.ToString())

            Try
                Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
            Catch ex1 As Exception
                mp.AppendLine("Save AHA Detail. Error on Try Catch...")
                If ex.StackTrace IsNot Nothing Then
                    mp.AppendLine("StactTrace...: " & ex1.StackTrace.ToString)
                End If

                Globals.LogError(ahaItem.SessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex1.Message.ToString(), "")
            End Try

            Return False
            'Throw

        End Try

    End Function


    Private Sub SavePage1(ByVal ahaItem As AHAFormItem, ByVal dbo As SqlDataObject)

        Try
            If ahaItem.FormHeader.AtHome.Value Is Nothing Then
                ahaItem.FormHeader.AtHome.Value = False
            End If

            SaveChiefComplaintPatMedHistorySection(ahaItem.ID, ahaItem.ChiefComplaintPatientMedicalHist, ahaItem.FormHeader.AtHome.Value, ahaItem.FormHeader.AccompaniedBy.Value, dbo, ahaItem.FormHeader.TypeOfVisit)
            SaveMedicalFamilySocialHistorySection(ahaItem.ID, ahaItem.ChiefComplaintPatientMedicalHist.MedicalFamilySocialHistory, dbo)
            SaveAdvanceDirectivesSection(ahaItem.ID, ahaItem.AdvanceDirective, dbo)
            SaveReviewOfSystemSection(ahaItem.ID, ahaItem.ReviewOfSystem, dbo)
            SaveMedicationListSection(ahaItem.ID, ahaItem.MedicationList, dbo)

            If _isGHP AndAlso _memberAge < 21 Then
                SaveAllergiesMedicationListSection(ahaItem.ID, ahaItem.MedicationList, dbo)
            End If

            SaveMyocardialInfarction(ahaItem.ID, ahaItem.MyocardialInfarction, dbo)

        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Private Sub SavePage2(ByVal ahaItem As AHAFormItem, ByVal dbo As SqlDataObject)

        Try
            SaveMedicationReviewPatientCaregiverSection(ahaItem.ID, ahaItem.MedicalReview, dbo)
            SaveCognitiveAssessmentSection(ahaItem.ID, ahaItem.CognitiveAssesment, dbo)
            SavePainScreeningSection(ahaItem.ID, ahaItem.PainScreening, dbo)
            SaveActivitiesDailyLiving(ahaItem.ID, ahaItem.ActivitiesOfDailyLiving, dbo)
        Catch ex As Exception
            Throw ex
        End Try

    End Sub

    Private Sub SavePage3(ByVal ahaItem As AHAFormItem, ByVal dbo As SqlDataObject)
        Try
            If IsNothing(ahaItem.FormHeader.ClaimClassTag) Then
                ahaItem.FormHeader.ClaimClassTag = New IntegerField With {.Value = 1}
            End If

            If (ahaItem.FormHeader.DateOfVisit.Value.Value.Year < 2023) Then 'OrElse _isGHP
                SaveScreeningTest(ahaItem.ID, ahaItem.ScreeningSchedule, dbo)
            Else
                SaveScreeningTest2023(ahaItem.ID, ahaItem.ScreeningSchedule, dbo)
            End If
            SavePhysicalExamination(ahaItem.ID, ahaItem.PhysicalExamination, dbo)

            'At Home
            If ahaItem.FormHeader.AtHome.Value.HasValue AndAlso ahaItem.FormHeader.AtHome.Value Then
                ' SaveDepressionInventorySection(ahaItem.ID, ahaItem.DepressionInventory, dbo)
                'SaveDMEUseSection(ahaItem.ID, ahaItem.DMEUse, dbo)
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Function IsTHAVitalPed(ByVal ahaItem As AHAFormItem) As Boolean
        Dim isTHA As Boolean = IIf(IsNothing(ahaItem.FormHeader.ClaimClassTag), 1, ahaItem.FormHeader.ClaimClassTag.Value) = 4
        Dim isGHP As Boolean = IIf(IsNothing(ahaItem.FormHeader.isGHP), False, IIf(IsNothing(ahaItem.FormHeader.isGHP.Value), False, ahaItem.FormHeader.isGHP.Value.Value))
        Dim memberAge As Decimal = TimeSpan.FromTicks(ahaItem.FormHeader.DateOfVisit.Value.Value.Ticks - ahaItem.FormHeader.MemberDOB.Value.Value.Ticks).TotalDays / 365.25
        Return isTHA AndAlso isGHP AndAlso memberAge < 21
    End Function

    Private Sub SavePage4(ByVal ahaItem As AHAFormItem, ByVal dbo As SqlDataObject)

        Try
            If IsNothing(ahaItem.ScreeningSchedule) Then
                ahaItem.ScreeningSchedule = New ScreeningScheduleSection() With {.IsDiabetic = New BooleanField() With {.Value = False}}
            End If
            'ahaItem.ScreeningSchedule = IIf(IsNothing(ahaItem.ScreeningSchedule), New BooleanField() With {.Value = False}, ahaItem.ScreeningSchedule)
            SaveAssessmentPlanTreatment(ahaItem.ID, ahaItem.AssessmentPlanOfTreatment, ahaItem.ScreeningSchedule.IsDiabetic, dbo) 'check and tested
            SaveCongenitalDiseases(ahaItem.ID, ahaItem.CongenitalDiseases, dbo)

            SaveCKD(ahaItem.ID, ahaItem.CKD, dbo) 'check and tested
            SavePressureSore(ahaItem.ID, ahaItem.PressureSores, dbo) 'check
            SaveRheumatoidArthritis(ahaItem.ID, ahaItem.RheumatoidArthritis, dbo) 'check

            'SaveMyocardialInfarction(ahaItem.ID, ahaItem.MyocardialInfarction, dbo)
            SaveBMIAssociatedDiagnoses(ahaItem.ID, ahaItem.BMIAssociatedDiagnoses, dbo) 'check

            SaveMajorDepression(ahaItem.ID, ahaItem.MajorDepression, dbo) 'check
            SavePressureSoreList(ahaItem.ID, ahaItem.PressureSoresList, dbo) 'check
            SaveDiseasesOfTheSkin(ahaItem.ID, ahaItem.DiseasesOfTheSkin, dbo, ahaItem.DiseasesOfTheSking_NA)

            SaveCancerDiagnosis(ahaItem.ID, ahaItem.CancerDiagnosis, dbo, ahaItem.CancerDiagnosis_NA) 'check
            SaveOtherCondition(ahaItem.ID, ahaItem.OtherCurrentConditions, dbo, ahaItem.FormHeader.DateOfVisit.Value.Value) 'check

            SaveCardiovascularDiseases(ahaItem.ID, ahaItem.CardiovascularDiseases, dbo) 'check
            'SaveEyeAndNeurology(ahaItem.ID, ahaItem.EyesAndNeurology, dbo)

            SavePulmonaryDiseases(ahaItem.ID, ahaItem.PulmonaryDiseases, dbo)
            SaveImLabRefSection(ahaItem.ID, ahaItem.ImLabRef, dbo)

            SaveClaimsMalnutritionCriteria(ahaItem.ID, ahaItem.MalnutritionCriteria, dbo)
            SaveClaimsScreeningSubstanceUse(ahaItem.ID, ahaItem.ScreeningSubstanceUseList, dbo)
            'SaveClaimsSocialDeterminants(ahaItem.ID, ahaItem.SocialDeterminants2020, dbo)
            'TODO: Se añadio el dato del CheckBox de SocialDetermiantsN/A

            'The social determinants survey changes compleatly for 2023, so new functions, models, and SPs where created for this.
            If ahaItem.FormHeader.DateOfVisit.Value.Value.Year < 2023 Then 'OrElse _isGHP
                SaveEyeAndNeurology(ahaItem.ID, ahaItem.EyesAndNeurology, dbo)
                SaveClaimsSocialDeterminants(ahaItem.ID, ahaItem.SocialDeterminants2020, ahaItem.SocialDeterminants_NA, dbo)
            Else
                If Not _isGHP Then
                    SaveEyeAndNeurology2023(ahaItem.ID, ahaItem.EyesAndNeurology, dbo)
                End If

                SaveClaimsSocialDeterminants2023(ahaItem.ID, ahaItem.SocialDeterminants2023, ahaItem.SocialDeterminants_NA, dbo)
            End If

            SaveScreeningResult(ahaItem.ID, ahaItem.ScreeningSubstanceUseListResult?.Value,
                            ahaItem.SocialDeterminants2023?.Social_Determinants_Result?.Value,
                            ahaItem.MalnutritionCriteria?.Malnutrition_Criteria_Result?.Value, dbo)

            SaveGastrointestinalDiseases(ahaItem.ID, ahaItem.GastrointestinalDiseases, dbo)

            If ahaItem.FormHeader.AtHome.Value.HasValue AndAlso ahaItem.FormHeader.AtHome.Value Then
                SaveOtherConditionAdditional(ahaItem.ID, ahaItem.OtherCurrentConditionsAdditional, dbo)
            End If

            'GHP only Sections
            If _isGHP Then
                SaveGastrointestinal(ahaItem.ID, ahaItem.Gastrointestinal, dbo)
                SaveMusculoskeletal(ahaItem.ID, ahaItem.Musculoskeletal, dbo)
            End If
        Catch ex As Exception
            Throw ex
        End Try

    End Sub

#Region " Page 1 Save Section"

    Private Sub SaveChiefComplaintPatMedHistorySection(ByVal claimKey As Long,
                                                       ByVal ccpmSection As ChiefComplaintPatientMedicalHistorySection, ByVal isAtHome As Boolean,
                                                       ByVal accompaniedBy As String, ByVal dbo As SqlDataObject, ByVal typeOfVisit As Integer)

        Dim cmd As New SqlClient.SqlCommand

        Try

            If ccpmSection IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveChiefComplaintPatientMedHistory"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("HistoryCurrentIllnesses", VerifyStringNull(ccpmSection.HistoryOfPresentIllness))
                    .AddWithValue("HasRecentHosp", VerifyBooleanNull(ccpmSection.RecentHospitalization))
                    .AddWithValue("RecentHospDate", VerifyDateNull(ccpmSection.RecentHospitalizationDate))
                    '.AddWithValue("HospDueFracture", VerifyBooleanNull(ccpmSection.RecentHospitalizationDueFracture.Value))
                    '.AddWithValue("HasRecentSurgery", VerifyBooleanNull(ccpmSection.RecentSurgery.Value))
                    '.AddWithValue("RecentSurgeryDate", VerifyDateNull(ccpmSection.RecentSurgeryDate.Value))
                    .AddWithValue("AllergiesNotes", VerifyStringNull(ccpmSection.AllergiesNotes))
                    '.AddWithValue("FamilySocialHistory", VerifyStringNull(ccpmSection.FamilySocialHistory.Value))

                    '***NO params
                    .AddWithValue("NoAllergies", VerifyBooleanNull(ccpmSection.NoAllergies))
                    .AddWithValue("TotalColectomy", VerifyBooleanNull(ccpmSection.TotalColectomy))
                    .AddWithValue("TotalColectomyDate", VerifyStringNull(ccpmSection.TotalColectomyDate))
                    .AddWithValue("BilateralMastectomy", VerifyBooleanNull(ccpmSection.BilateralMastectomy))
                    .AddWithValue("BilateralMastectomyDate", VerifyStringNull(ccpmSection.BilateralMastectomyDate))
                    .AddWithValue("OtherSurgery", VerifyStringNull(ccpmSection.OtherSurgery))

                    .AddWithValue("OtherSurgeryDate", VerifyStringNull(ccpmSection.OtherSurgeryDate))
                    .AddWithValue("NoSurgery", VerifyBooleanNull(ccpmSection.NoSurgery))
                    .AddWithValue("AtHome", isAtHome)

                    .AddWithValue("UnilateralMastectomyLeft", VerifyBooleanNull(ccpmSection.UnilateralMastectomyLeft))
                    .AddWithValue("UnilateralMastectomyLeftDate", VerifyStringNull(ccpmSection.UnilateralMastectomyLeftDate))

                    .AddWithValue("UnilateralMastectomyRight", VerifyBooleanNull(ccpmSection.UnilateralMastectomyRight))
                    .AddWithValue("UnilateralMastectomyRightDate", VerifyStringNull(ccpmSection.UnilateralMastectomyRightDate))

                    .AddWithValue("HistoryPresentIllnessSelectedText", ccpmSection.HistoryPresentIllnessSelectedText.Value)

                    .AddWithValue("AccompaniedBy", accompaniedBy)

                    .AddWithValue("TypeOfVisit", typeOfVisit)

                    .AddWithValue("choseRiskofHIV", VerifyBooleanNull(ccpmSection.ChoseRiskofHIV))
                    .AddWithValue("choseOtherSTD", VerifyBooleanNull(ccpmSection.ChoseOtherSTD))
                    .AddWithValue("choseQuittingTabacco", VerifyBooleanNull(ccpmSection.ChoseQuittingTabacco))
                    .AddWithValue("choseDrinkingAlcohol", VerifyBooleanNull(ccpmSection.ChoseDrinkingAlcohol))
                    .AddWithValue("choseUseIllicitDrugs", VerifyBooleanNull(ccpmSection.ChoseUseIllicitDrugs))

                    If ccpmSection.PatientConcentTelecomm IsNot Nothing AndAlso ccpmSection.PatientConcentTelecomm.Value IsNot Nothing Then
                        .AddWithValue("PatientConcentTelecomm", ccpmSection.PatientConcentTelecomm.Value.Value)
                    End If

                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
            Dim errD = New Dictionary(Of String, String)

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveMedicalFamilySocialHistorySection(ByVal claimKey As Long,
                                                      ByVal medFamSocialHist As MedicalFamilySocialHistorySection, ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If medFamSocialHist IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMedicalFamilySocialHistory"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)

                    If medFamSocialHist.NA IsNot Nothing Then .AddWithValue("NA", VerifyBooleanNull(medFamSocialHist.NA))
                    .AddWithValue("DMPatient", VerifyBooleanNull(medFamSocialHist.DMPatient))
                    .AddWithValue("DMMother", VerifyBooleanNull(medFamSocialHist.DMMother))
                    .AddWithValue("DMFather", VerifyBooleanNull(medFamSocialHist.DMFather))
                    .AddWithValue("DMSiblings", VerifyBooleanNull(medFamSocialHist.DMSiblings))

                    .AddWithValue("CVDPatient", VerifyBooleanNull(medFamSocialHist.CVDPatient))
                    .AddWithValue("CVDMother", VerifyBooleanNull(medFamSocialHist.CVDMother))
                    .AddWithValue("CVDFather", VerifyBooleanNull(medFamSocialHist.CVDFather))
                    .AddWithValue("CVDSiblings", VerifyBooleanNull(medFamSocialHist.CVDSiblings))

                    .AddWithValue("CholPatient", VerifyBooleanNull(medFamSocialHist.CholesterolPatient))
                    .AddWithValue("CholMother", VerifyBooleanNull(medFamSocialHist.CholesterolMother))
                    .AddWithValue("CholFather", VerifyBooleanNull(medFamSocialHist.CholesterolFather))
                    .AddWithValue("CholSiblings", VerifyBooleanNull(medFamSocialHist.CholesterolSiblings))

                    .AddWithValue("CancerPatient", VerifyBooleanNull(medFamSocialHist.CancerPatient))
                    .AddWithValue("CancerMother", VerifyBooleanNull(medFamSocialHist.CancerMother))
                    .AddWithValue("CancerFather", VerifyBooleanNull(medFamSocialHist.CancerFather))
                    .AddWithValue("CancerSiblings", VerifyBooleanNull(medFamSocialHist.CancerSiblings))

                    .AddWithValue("AlzheimerPatient", VerifyBooleanNull(medFamSocialHist.AlzheimerPatient))
                    .AddWithValue("AlzheimerMother", VerifyBooleanNull(medFamSocialHist.AlzheimerMother))
                    .AddWithValue("AlzheimerFather", VerifyBooleanNull(medFamSocialHist.AlzheimerFather))
                    .AddWithValue("AlzheimerSiblings", VerifyBooleanNull(medFamSocialHist.AlzheimerSiblings))

                    .AddWithValue("VIHPatient", VerifyBooleanNull(medFamSocialHist.VIHPatient))
                    .AddWithValue("VIHMother", VerifyBooleanNull(medFamSocialHist.VIHMother))
                    .AddWithValue("VIHFather", VerifyBooleanNull(medFamSocialHist.VIHFather))
                    .AddWithValue("VIHSiblings", VerifyBooleanNull(medFamSocialHist.VIHSiblings))

                    .AddWithValue("RiskForHIV", VerifyBooleanNull(medFamSocialHist.RiskForHIV))
                    .AddWithValue("RiskForSTD", VerifyBooleanNull(medFamSocialHist.RiskForSTD))
                    .AddWithValue("CounselTabaccoUse", VerifyBooleanNull(medFamSocialHist.CounselTabaccoUse))
                    .AddWithValue("CounselIllicitDrugUse", VerifyBooleanNull(medFamSocialHist.CounselIllicitDrugUse))
                    .AddWithValue("CounselAlcoholUse", VerifyBooleanNull(medFamSocialHist.CounselAlcoholUse))

                    If medFamSocialHist.PatientNA IsNot Nothing Then .AddWithValue("PatientNA", VerifyBooleanNull(medFamSocialHist.PatientNA))
                    If medFamSocialHist.MotherNA IsNot Nothing Then .AddWithValue("MotherNA", VerifyBooleanNull(medFamSocialHist.MotherNA))
                    If medFamSocialHist.FatherNA IsNot Nothing Then .AddWithValue("FatherNA", VerifyBooleanNull(medFamSocialHist.FatherNA))
                    If medFamSocialHist.SibligsNA IsNot Nothing Then .AddWithValue("BrotherNA", VerifyBooleanNull(medFamSocialHist.SibligsNA))

                    If medFamSocialHist.HistoryAlcoholism IsNot Nothing Then .AddWithValue("HistoryAlcoholism", VerifyBooleanNull(medFamSocialHist.HistoryAlcoholism))
                    If medFamSocialHist.HistoryDrugDependence IsNot Nothing Then .AddWithValue("HistoryDrugDependence", VerifyBooleanNull(medFamSocialHist.HistoryDrugDependence))
                    If medFamSocialHist.HistoryCaffeineDependence IsNot Nothing Then .AddWithValue("HistoryCaffeineDependence", VerifyBooleanNull(medFamSocialHist.HistoryCaffeineDependence))

                    If medFamSocialHist.Nicotine IsNot Nothing Then .AddWithValue("Nicotine", VerifyBooleanNull(medFamSocialHist.Nicotine))
                    If medFamSocialHist.Opiates IsNot Nothing Then .AddWithValue("Opiates", VerifyBooleanNull(medFamSocialHist.Opiates))
                    If medFamSocialHist.Cannabis IsNot Nothing Then .AddWithValue("Cannabis", VerifyBooleanNull(medFamSocialHist.Cannabis))
                    If medFamSocialHist.Sedatives IsNot Nothing Then .AddWithValue("Sedatives", VerifyBooleanNull(medFamSocialHist.Sedatives))
                    If medFamSocialHist.Hypnotics IsNot Nothing Then .AddWithValue("Hypnotics", VerifyBooleanNull(medFamSocialHist.Hypnotics))
                    If medFamSocialHist.Anxiolytics IsNot Nothing Then .AddWithValue("Anxiolytics", VerifyBooleanNull(medFamSocialHist.Anxiolytics))
                    If medFamSocialHist.OtherDrugs IsNot Nothing Then .AddWithValue("OtherDrugs", VerifyBooleanNull(medFamSocialHist.OtherDrugs))
                    If medFamSocialHist.OtherDrugsText IsNot Nothing Then .AddWithValue("OtherDrugsText", VerifyStringNull(medFamSocialHist.OtherDrugsText))

                    If medFamSocialHist.OtherConditionText IsNot Nothing Then .AddWithValue("OtherConditionText", VerifyStringNull(medFamSocialHist.OtherConditionText))
                    If medFamSocialHist.OtherPatient IsNot Nothing Then .AddWithValue("OtherPatient", VerifyBooleanNull(medFamSocialHist.OtherPatient))
                    If medFamSocialHist.OtherMother IsNot Nothing Then .AddWithValue("OtherMother", VerifyBooleanNull(medFamSocialHist.OtherMother))
                    If medFamSocialHist.OtherFather IsNot Nothing Then .AddWithValue("OtherFather", VerifyBooleanNull(medFamSocialHist.OtherFather))
                    If medFamSocialHist.OtherSiblings IsNot Nothing Then .AddWithValue("OtherSiblings", VerifyBooleanNull(medFamSocialHist.OtherSiblings))

                    If medFamSocialHist.HistoryOfMotherPregnancy IsNot Nothing Then .AddWithValue("HistoryOfMotherPregnancy", VerifyStringNull(medFamSocialHist.HistoryOfMotherPregnancy))
                    If medFamSocialHist.FisicalActivityScreening IsNot Nothing Then .AddWithValue("FisicalActivityScreening", VerifyStringNull(medFamSocialHist.FisicalActivityScreening))
                    If medFamSocialHist.FisicalActivityScreening_NA IsNot Nothing Then .AddWithValue("FisicalActivityScreening_NA", VerifyBooleanNull(medFamSocialHist.FisicalActivityScreening_NA))
                    If medFamSocialHist.FisicalActivityScreening_DailyActivityRecommended IsNot Nothing Then .AddWithValue("FisicalActivityScreening_DailyActivityRecommended", VerifyBooleanNull(medFamSocialHist.FisicalActivityScreening_DailyActivityRecommended))

                    If medFamSocialHist.NutricionalScreening_NA IsNot Nothing Then .AddWithValue("NutricionalScreening_NA", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_NA))
                    If medFamSocialHist.NutricionalScreening_AdequateIntake IsNot Nothing Then .AddWithValue("NutricionalScreening_AdequateIntake", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_AdequateIntake))
                    If medFamSocialHist.NutricionalScreening_BalancedNutriciousDiet IsNot Nothing Then .AddWithValue("NutricionalScreening_BalancedNutritiousDiet", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_BalancedNutriciousDiet))
                    If medFamSocialHist.NutricionalScreening_Breastmilk IsNot Nothing Then .AddWithValue("NutricionalScreening_Breastmilk", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_Breastmilk))
                    If medFamSocialHist.NutricionalScreening_Cereal IsNot Nothing Then .AddWithValue("NutricionalScreening_Cereal", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_Cereal))
                    If medFamSocialHist.NutricionalScreening_CowMilk IsNot Nothing Then .AddWithValue("NutricionalScreening_CowMilk", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_CowMilk))
                    If medFamSocialHist.NutricionalScreening_FeedsItselft IsNot Nothing Then .AddWithValue("NutricionalScreening_FeedsItself", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_FeedsItselft))
                    If medFamSocialHist.NutricionalScreening_Formula IsNot Nothing Then .AddWithValue("NutricionalScreening_Formula", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_Formula))
                    If medFamSocialHist.NutricionalScreening_JunkFood IsNot Nothing Then .AddWithValue("NutricionalScreening_JunkFood", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_JunkFood))
                    If medFamSocialHist.NutricionalScreening_Other IsNot Nothing Then .AddWithValue("NutricionalScreening_Others", VerifyStringNull(medFamSocialHist.NutricionalScreening_Other))
                    If medFamSocialHist.NutricionalScreening_Overweight IsNot Nothing Then .AddWithValue("NutricionalScreening_Overweight", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_Overweight))
                    If medFamSocialHist.NutricionalScreening_SodaJuices IsNot Nothing Then .AddWithValue("NutricionalScreening_SodaJuices", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_SodaJuices))
                    If medFamSocialHist.NutricionalScreening_SolidFoot IsNot Nothing Then .AddWithValue("NutricionalScreening_SolidFoot", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_SolidFoot))
                    If medFamSocialHist.NutricionalScreening_SupplementVitamins IsNot Nothing Then .AddWithValue("NutricionalScreening_SupplementVitamins", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_SupplementVitamins))
                    If medFamSocialHist.NutricionalScreening_UnderWeight IsNot Nothing Then .AddWithValue("NutricionalScreening_UnderWeight", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_UnderWeight))
                    If medFamSocialHist.NutricionalScreening_FoodAllergies IsNot Nothing Then .AddWithValue("NutricionalScreening_FoodAllergies", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_FoodAllergies))
                    If medFamSocialHist.NutricionalScreening_SpecialDiets IsNot Nothing Then .AddWithValue("NutricionalScreening_SpecialDiets", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_SpecialDiets))
                    If medFamSocialHist.NutricionalScreening_Others_Checkbox IsNot Nothing Then .AddWithValue("NutricionalScreening_Others_Checkbox", VerifyBooleanNull(medFamSocialHist.NutricionalScreening_Others_Checkbox))

                    'Development Screening
                    If medFamSocialHist.DevelopmentHealth_NA IsNot Nothing Then .AddWithValue("DevelopmentHealth_NA", VerifyBooleanNull(medFamSocialHist.DevelopmentHealth_NA))
                    If medFamSocialHist.DevelopmentScreening_CommunicationArea IsNot Nothing Then .AddWithValue("DevelopmentScreening_CommunicationArea", VerifyBooleanNull(medFamSocialHist.DevelopmentScreening_CommunicationArea))
                    If medFamSocialHist.DevelopmentScreening_FineMotorSkillArea IsNot Nothing Then .AddWithValue("DevelopmentScreening_FineMotorSkillArea", VerifyBooleanNull(medFamSocialHist.DevelopmentScreening_FineMotorSkillArea))
                    If medFamSocialHist.DevelopmentScreening_GrossMotorSkillsArea IsNot Nothing Then .AddWithValue("DevelopmentScreening_GrossMotorSkillsArea", VerifyBooleanNull(medFamSocialHist.DevelopmentScreening_GrossMotorSkillsArea))
                    If medFamSocialHist.DevelopmentScreening_SocialIndividualSkillsArea IsNot Nothing Then .AddWithValue("DevelopmentScreening_SocialIndividualSkillsArea", VerifyBooleanNull(medFamSocialHist.DevelopmentScreening_SocialIndividualSkillsArea))
                    If medFamSocialHist.DevelopmentScreening_ProblemResolutionSkillArea IsNot Nothing Then .AddWithValue("DevelopmentScreening_ProblemResolutionSkillArea", VerifyBooleanNull(medFamSocialHist.DevelopmentScreening_ProblemResolutionSkillArea))
                    If medFamSocialHist.DevelopmentScreening_BehavioralHealthArea IsNot Nothing Then .AddWithValue("DevelopmentScreening_BehavioralHealthArea", VerifyBooleanNull(medFamSocialHist.DevelopmentScreening_BehavioralHealthArea))

                    If medFamSocialHist.BehavioralHealth_NA IsNot Nothing Then .AddWithValue("BehavioralHealth_NA", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_NA))
                    If medFamSocialHist.BehavioralHealth_PhysicalMentalSelftRegulation IsNot Nothing Then .AddWithValue("BehavioralHealth_PhysicalMentalSelftRegulation", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_PhysicalMentalSelftRegulation))
                    If medFamSocialHist.BehavioralHealth_HabilityToFollowsInstructionsRules IsNot Nothing Then .AddWithValue("BehavioralHealth_HabilityToFollowsInstructionsRules", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_HabilityToFollowsInstructionsRules))
                    If medFamSocialHist.BehavioralHealth_SocialCommunication IsNot Nothing Then .AddWithValue("BehavioralHealth_SocialCommunication", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_SocialCommunication))
                    If medFamSocialHist.BehavioralHealth_AdaptativeFunctioning IsNot Nothing Then .AddWithValue("BehavioralHealth_AdaptativeFunctioning", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_AdaptativeFunctioning))
                    If medFamSocialHist.BehavioralHealth_Autonomy IsNot Nothing Then .AddWithValue("BehavioralHealth_Autonomy", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_Autonomy))
                    If medFamSocialHist.BehavioralHealth_CapacityToBeAffectiveEmpathic IsNot Nothing Then .AddWithValue("BehavioralHealth_CapacityToBeAffectiveEmpathic", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_CapacityToBeAffectiveEmpathic))
                    If medFamSocialHist.BehavioralHealth_InteractionWithPeople IsNot Nothing Then .AddWithValue("BehavioralHealth_InteractionWithPeople", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_InteractionWithPeople))
                    If medFamSocialHist.BehavioralHealth_UsesAlcoholDrugs IsNot Nothing Then .AddWithValue("BehavioralHealth_UsesAlcoholDrugs", VerifyBooleanNull(medFamSocialHist.BehavioralHealth_UsesAlcoholDrugs))

                    If medFamSocialHist.AppropriateEducation_NA IsNot Nothing Then .AddWithValue("AppropriateEducation_NA", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_NA))
                    If medFamSocialHist.AppropriateEducation_AppropiateUseCarSeat IsNot Nothing Then .AddWithValue("AppropriateEducation_AppropiateUseCarSeat", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_AppropiateUseCarSeat))
                    If medFamSocialHist.AppropriateEducation_BottleProp IsNot Nothing Then .AddWithValue("AppropriateEducation_BottleProp", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_BottleProp))
                    If medFamSocialHist.AppropriateEducation_PasiveSmoke IsNot Nothing Then .AddWithValue("AppropriateEducation_PasiveSmoke", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_PasiveSmoke))
                    If medFamSocialHist.AppropriateEducation_InfantCryingWhatToDo IsNot Nothing Then .AddWithValue("AppropriateEducation_InfantCryingWhatToDo", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_InfantCryingWhatToDo))
                    If medFamSocialHist.AppropriateEducation_ShakeBabyPrevention IsNot Nothing Then .AddWithValue("AppropriateEducation_ShakeBabyPrevention", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_ShakeBabyPrevention))
                    If medFamSocialHist.AppropriateEducation_Firearm IsNot Nothing Then .AddWithValue("AppropriateEducation_Firearm", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_Firearm))
                    If medFamSocialHist.AppropriateEducation_Pacifiers IsNot Nothing Then .AddWithValue("AppropriateEducation_Pacifiers", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_Pacifiers))
                    If medFamSocialHist.AppropriateEducation_ParentsReadToChild IsNot Nothing Then .AddWithValue("AppropriateEducation_ParentsReadToChild", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_ParentsReadToChild))
                    If medFamSocialHist.AppropriateEducation_Emergency911 IsNot Nothing Then .AddWithValue("AppropriateEducation_Emergency911", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_Emergency911))
                    If medFamSocialHist.AppropriateEducation_FingerFoodChoking IsNot Nothing Then .AddWithValue("AppropriateEducation_FingerFoodChoking", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_FingerFoodChoking))
                    If medFamSocialHist.AppropriateEducation_DisciplinePrais IsNot Nothing Then .AddWithValue("AppropriateEducation_DisciplinePrais", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_DisciplinePrais))
                    If medFamSocialHist.AppropriateEducation_DrowningPrevention IsNot Nothing Then .AddWithValue("AppropriateEducation_DrowningPrevention", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_DrowningPrevention))
                    If medFamSocialHist.AppropriateEducation_NeverLeaveToddlerAlone IsNot Nothing Then .AddWithValue("AppropriateEducation_NeverLeaveToddlerAlone", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_NeverLeaveToddlerAlone))
                    If medFamSocialHist.AppropriateEducation_ToiletTraining IsNot Nothing Then .AddWithValue("AppropriateEducation_ToiletTraining", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_ToiletTraining))
                    If medFamSocialHist.AppropriateEducation_NutritionExercise IsNot Nothing Then .AddWithValue("AppropriateEducation_NutritionExercise", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_NutritionExercise))
                    If medFamSocialHist.AppropriateEducation_EstablishRoutineBedMealsToiletingEtc IsNot Nothing Then .AddWithValue("AppropriateEducation_EstablishRoutineBedMealsToiletingEtc", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_EstablishRoutineBedMealsToiletingEtc))
                    If medFamSocialHist.AppropriateEducation_UseSportProtection IsNot Nothing Then .AddWithValue("AppropriateEducation_UseSportProtection", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_UseSportProtection))
                    If medFamSocialHist.AppropriateEducation_Bullying IsNot Nothing Then .AddWithValue("AppropriateEducation_Bullying", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_Bullying))
                    If medFamSocialHist.AppropriateEducation_OralHealth IsNot Nothing Then .AddWithValue("AppropriateEducation_OralHealth", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_OralHealth))
                    If medFamSocialHist.AppropriateEducation_Others IsNot Nothing Then .AddWithValue("AppropriateEducation_Others", VerifyStringNull(medFamSocialHist.AppropriateEducation_Others))
                    If medFamSocialHist.AppropriateEducation_SportInjuryPrevention IsNot Nothing Then .AddWithValue("AppropriateEducation_SportInjuryPrevention", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_SportInjuryPrevention))
                    If medFamSocialHist.AppropriateEducation_DrowningSunSafety IsNot Nothing Then .AddWithValue("AppropriateEducation_DrowningSunSafety", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_DrowningSunSafety))
                    If medFamSocialHist.AppropriateEducation_SafeAtHome IsNot Nothing Then .AddWithValue("AppropriateEducation_SafeAtHome", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_SafeAtHome))
                    If medFamSocialHist.AppropriateEducation_CorrectUseSeatbelt IsNot Nothing Then .AddWithValue("AppropriateEducation_CorrectUseSeatbelt", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_CorrectUseSeatbelt))
                    If medFamSocialHist.AppropriateEducation_SexualEducationSTD IsNot Nothing Then .AddWithValue("AppropriateEducation_SexualEducationSTD", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_SexualEducationSTD))
                    If medFamSocialHist.AppropriateEducation_DepresionAnxiety IsNot Nothing Then .AddWithValue("AppropriateEducation_DepresionAnxiety", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_DepresionAnxiety))
                    If medFamSocialHist.AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants IsNot Nothing Then .AddWithValue("AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_TabaccoAlcoholDrugsRxDrugsInhalants))
                    If medFamSocialHist.AppropriateEducation_RiskOfTattoosPiercing IsNot Nothing Then .AddWithValue("AppropriateEducation_RiskOfTattoosPiercing", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_RiskOfTattoosPiercing))
                    If medFamSocialHist.AppropriateEducation_Autocontrol IsNot Nothing Then .AddWithValue("AppropriateEducation_Autocontrol", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_Autocontrol))
                    If medFamSocialHist.AppropriateEducation_Others_Checkbox IsNot Nothing Then .AddWithValue("AppropriateEducation_Others_Checkbox", VerifyBooleanNull(medFamSocialHist.AppropriateEducation_Others_Checkbox))

                End With

                dbo.ExecuteCommand(cmd)

            End If

        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveImLabRefSection(ByVal claimKey As Long, ByVal imLabRefSec As ImLabRef, ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If imLabRefSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveImLabRef"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)

                    If imLabRefSec.Immunizations_NA IsNot Nothing Then .AddWithValue("Immunizations_NA", VerifyBooleanNull(imLabRefSec.Immunizations_NA))
                    If imLabRefSec.Immunizations_ParentRefuses IsNot Nothing Then .AddWithValue("Immunizations_ParentRefuses", VerifyBooleanNull(imLabRefSec.Immunizations_ParentRefuses))
                    If imLabRefSec.Immunizations_HepB1dose IsNot Nothing Then .AddWithValue("Immunizations_HepB1dose", VerifyBooleanNull(imLabRefSec.Immunizations_HepB1dose))
                    If imLabRefSec.Immunizations_HebB2dose IsNot Nothing Then .AddWithValue("Immunizations_HebB2dose", VerifyBooleanNull(imLabRefSec.Immunizations_HebB2dose))
                    If imLabRefSec.Immunizations_HebB3dose IsNot Nothing Then .AddWithValue("Immunizations_HebB3dose", VerifyBooleanNull(imLabRefSec.Immunizations_HebB3dose))
                    If imLabRefSec.Immunizations_HepA1dose IsNot Nothing Then .AddWithValue("Immunizations_HepA1dose", VerifyBooleanNull(imLabRefSec.Immunizations_HepA1dose))
                    If imLabRefSec.Immunizations_HebA2dose IsNot Nothing Then .AddWithValue("Immunizations_HebA2dose", VerifyBooleanNull(imLabRefSec.Immunizations_HebA2dose))
                    If imLabRefSec.Immunizations_DTaP1dose IsNot Nothing Then .AddWithValue("Immunizations_DTaP1dose", VerifyBooleanNull(imLabRefSec.Immunizations_DTaP1dose))
                    If imLabRefSec.Immunizations_DTaP2dose IsNot Nothing Then .AddWithValue("Immunizations_DTaP2dose", VerifyBooleanNull(imLabRefSec.Immunizations_DTaP2dose))
                    If imLabRefSec.Immunizations_DTaP3dose IsNot Nothing Then .AddWithValue("Immunizations_DTaP3dose", VerifyBooleanNull(imLabRefSec.Immunizations_DTaP3dose))
                    If imLabRefSec.Immunizations_DTaP4dose IsNot Nothing Then .AddWithValue("Immunizations_DTaP4dose", VerifyBooleanNull(imLabRefSec.Immunizations_DTaP4dose))
                    If imLabRefSec.Immunizations_DTaP5dose IsNot Nothing Then .AddWithValue("Immunizations_DTaP5dose", VerifyBooleanNull(imLabRefSec.Immunizations_DTaP5dose))
                    If imLabRefSec.Immunizations_Hib1dose IsNot Nothing Then .AddWithValue("Immunizations_Hib1dose", VerifyBooleanNull(imLabRefSec.Immunizations_Hib1dose))
                    If imLabRefSec.Immunizations_Hib2dose IsNot Nothing Then .AddWithValue("Immunizations_Hib2dose", VerifyBooleanNull(imLabRefSec.Immunizations_Hib2dose))
                    If imLabRefSec.Immunizations_Hib3dose IsNot Nothing Then .AddWithValue("Immunizations_Hib3dose", VerifyBooleanNull(imLabRefSec.Immunizations_Hib3dose))
                    If imLabRefSec.Immunizations_Hib4dose IsNot Nothing Then .AddWithValue("Immunizations_Hib4dose", VerifyBooleanNull(imLabRefSec.Immunizations_Hib4dose))
                    If imLabRefSec.Immunizations_PCV13_1dose IsNot Nothing Then .AddWithValue("Immunizations_PCV13_1dose", VerifyBooleanNull(imLabRefSec.Immunizations_PCV13_1dose))
                    If imLabRefSec.Immunizations_PCV13_2dose IsNot Nothing Then .AddWithValue("Immunizations_PCV13_2dose", VerifyBooleanNull(imLabRefSec.Immunizations_PCV13_2dose))
                    If imLabRefSec.Immunizations_PCV13_3dose IsNot Nothing Then .AddWithValue("Immunizations_PCV13_3dose", VerifyBooleanNull(imLabRefSec.Immunizations_PCV13_3dose))
                    If imLabRefSec.Immunizations_PCV13_4dose IsNot Nothing Then .AddWithValue("Immunizations_PCV13_4dose", VerifyBooleanNull(imLabRefSec.Immunizations_PCV13_4dose))
                    If imLabRefSec.Immunizations_IPV1dose IsNot Nothing Then .AddWithValue("Immunizations_IPV1dose", VerifyBooleanNull(imLabRefSec.Immunizations_IPV1dose))
                    If imLabRefSec.Immunizations_IPV2dose IsNot Nothing Then .AddWithValue("Immunizations_IPV2dose", VerifyBooleanNull(imLabRefSec.Immunizations_IPV2dose))
                    If imLabRefSec.Immunizations_IPV3dose IsNot Nothing Then .AddWithValue("Immunizations_IPV3dose", VerifyBooleanNull(imLabRefSec.Immunizations_IPV3dose))
                    If imLabRefSec.Immunizations_IPV4dose IsNot Nothing Then .AddWithValue("Immunizations_IPV4dose", VerifyBooleanNull(imLabRefSec.Immunizations_IPV4dose))
                    If imLabRefSec.Immunizations_MMR1dose IsNot Nothing Then .AddWithValue("Immunizations_MMR1dose", VerifyBooleanNull(imLabRefSec.Immunizations_MMR1dose))
                    If imLabRefSec.Immunizations_MMR2dose IsNot Nothing Then .AddWithValue("Immunizations_MMR2dose", VerifyBooleanNull(imLabRefSec.Immunizations_MMR2dose))
                    If imLabRefSec.Immunizations_Varicella1dose IsNot Nothing Then .AddWithValue("Immunizations_Varicella1dose", VerifyBooleanNull(imLabRefSec.Immunizations_Varicella1dose))
                    If imLabRefSec.Immunizations_Varicella2dose IsNot Nothing Then .AddWithValue("Immunizations_Varicella2dose", VerifyBooleanNull(imLabRefSec.Immunizations_Varicella2dose))
                    If imLabRefSec.Immunizations_Tdap IsNot Nothing Then .AddWithValue("Immunizations_Tdap", VerifyBooleanNull(imLabRefSec.Immunizations_Tdap))
                    If imLabRefSec.Immunizations_Rotavirus1dose IsNot Nothing Then .AddWithValue("Immunizations_Rotavirus1dose", VerifyBooleanNull(imLabRefSec.Immunizations_Rotavirus1dose))
                    If imLabRefSec.Immunizations_Rotavirus2dose IsNot Nothing Then .AddWithValue("Immunizations_Rotavirus2dose", VerifyBooleanNull(imLabRefSec.Immunizations_Rotavirus2dose))
                    If imLabRefSec.Immunizations_Influenza IsNot Nothing Then .AddWithValue("Immunizations_Influenza", VerifyBooleanNull(imLabRefSec.Immunizations_Influenza))
                    If imLabRefSec.Immunizations_MenningococcalMCV IsNot Nothing Then .AddWithValue("Immunizations_MenningococcalMCV", VerifyBooleanNull(imLabRefSec.Immunizations_MenningococcalMCV))
                    If imLabRefSec.Immunizations_HPV1dose IsNot Nothing Then .AddWithValue("Immunizations_HPV1dose", VerifyBooleanNull(imLabRefSec.Immunizations_HPV1dose))
                    If imLabRefSec.Immunizations_HPV2dose IsNot Nothing Then .AddWithValue("Immunizations_HPV2dose", VerifyBooleanNull(imLabRefSec.Immunizations_HPV2dose))
                    If imLabRefSec.Immunizations_HPV3dose IsNot Nothing Then .AddWithValue("Immunizations_HPV3dose", VerifyBooleanNull(imLabRefSec.Immunizations_HPV3dose))
                    If imLabRefSec.Immunizations_Others IsNot Nothing Then .AddWithValue("Immunizations_Others", VerifyStringNull(imLabRefSec.Immunizations_Others))

                    If imLabRefSec.Lab_NA IsNot Nothing Then .AddWithValue("Lab_NA", VerifyBooleanNull(imLabRefSec.Lab_NA))
                    If imLabRefSec.Lab_HgbHct IsNot Nothing Then .AddWithValue("Lab_HgbHct", VerifyBooleanNull(imLabRefSec.Lab_HgbHct))
                    If imLabRefSec.Lab_TB IsNot Nothing Then .AddWithValue("Lab_TB", VerifyBooleanNull(imLabRefSec.Lab_TB))
                    If imLabRefSec.Lab_UA IsNot Nothing Then .AddWithValue("Lab_UA", VerifyBooleanNull(imLabRefSec.Lab_UA))
                    If imLabRefSec.Lab_LipidProfile IsNot Nothing Then .AddWithValue("Lab_LipidProfile", VerifyBooleanNull(imLabRefSec.Lab_LipidProfile))
                    If imLabRefSec.Lab_BloodLeadTest IsNot Nothing Then .AddWithValue("Lab_BloodLeadTest", VerifyBooleanNull(imLabRefSec.Lab_BloodLeadTest))
                    If imLabRefSec.Lab_VIH IsNot Nothing Then .AddWithValue("Lab_VIH", VerifyBooleanNull(imLabRefSec.Lab_VIH))
                    If imLabRefSec.Lab_NAAT IsNot Nothing Then .AddWithValue("Lab_NAAT", VerifyBooleanNull(imLabRefSec.Lab_NAAT))
                    If imLabRefSec.Lab_VDRL IsNot Nothing Then .AddWithValue("Lab_VDRL", VerifyBooleanNull(imLabRefSec.Lab_VDRL))
                    If imLabRefSec.Lab_Other IsNot Nothing Then .AddWithValue("Lab_Other", VerifyStringNull(imLabRefSec.Lab_Other))

                    'If imLabRefSec.Lab_HgbHct_Ordered IsNot Nothing Then .AddWithValue("Lab_HgbHct_Ordered", VerifyBooleanNull(imLabRefSec.Lab_HgbHct_Ordered))
                    'If imLabRefSec.Lab_TB_Ordered IsNot Nothing Then .AddWithValue("Lab_TB_Ordered", VerifyBooleanNull(imLabRefSec.Lab_TB_Ordered))
                    'If imLabRefSec.Lab_UA_Ordered IsNot Nothing Then .AddWithValue("Lab_UA_Ordered", VerifyBooleanNull(imLabRefSec.Lab_UA_Ordered))
                    'If imLabRefSec.Lab_LipidProfile_Ordered IsNot Nothing Then .AddWithValue("Lab_LipidProfile_Ordered", VerifyBooleanNull(imLabRefSec.Lab_LipidProfile_Ordered))
                    'If imLabRefSec.Lab_BloodLeadTest_Ordered IsNot Nothing Then .AddWithValue("Lab_BloodLeadTest_Ordered", VerifyBooleanNull(imLabRefSec.Lab_BloodLeadTest_Ordered))
                    'If imLabRefSec.Lab_VIH_Ordered IsNot Nothing Then .AddWithValue("Lab_VIH_Ordered", VerifyBooleanNull(imLabRefSec.Lab_VIH_Ordered))
                    'If imLabRefSec.Lab_NAAT_Ordered IsNot Nothing Then .AddWithValue("Lab_NAAT_Ordered", VerifyBooleanNull(imLabRefSec.Lab_NAAT_Ordered))
                    'If imLabRefSec.Lab_VDRL_Ordered IsNot Nothing Then .AddWithValue("Lab_VDRL_Ordered", VerifyBooleanNull(imLabRefSec.Lab_VDRL_Ordered))
                    'If imLabRefSec.Lab_Other_Ordered IsNot Nothing Then .AddWithValue("Lab_Other_Ordered", VerifyBooleanNull(imLabRefSec.Lab_Other_Ordered))
                    'If imLabRefSec.Lab_HgbHct_Result IsNot Nothing Then .AddWithValue("Lab_HgbHct_Result", VerifyStringNull(imLabRefSec.Lab_HgbHct_Result))
                    'If imLabRefSec.Lab_TB_Result IsNot Nothing Then .AddWithValue("Lab_TB_Result", VerifyStringNull(imLabRefSec.Lab_TB_Result))
                    'If imLabRefSec.Lab_UA_Result IsNot Nothing Then .AddWithValue("Lab_UA_Result", VerifyStringNull(imLabRefSec.Lab_UA_Result))
                    'If imLabRefSec.Lab_LipidProfile_Result IsNot Nothing Then .AddWithValue("Lab_LipidProfile_Result", VerifyStringNull(imLabRefSec.Lab_LipidProfile_Result))
                    'If imLabRefSec.Lab_BloodLeadTest_Result IsNot Nothing Then .AddWithValue("Lab_BloodLeadTest_Result", VerifyStringNull(imLabRefSec.Lab_BloodLeadTest_Result))
                    'If imLabRefSec.Lab_VIH_Result IsNot Nothing Then .AddWithValue("Lab_VIH_Result", VerifyStringNull(imLabRefSec.Lab_VIH_Result))
                    'If imLabRefSec.Lab_NAAT_Result IsNot Nothing Then .AddWithValue("Lab_NAAT_Result", VerifyStringNull(imLabRefSec.Lab_NAAT_Result))
                    'If imLabRefSec.Lab_VDRL_Result IsNot Nothing Then .AddWithValue("Lab_VDRL_Result", VerifyStringNull(imLabRefSec.Lab_VDRL_Result))
                    'If imLabRefSec.Lab_Other_Result IsNot Nothing Then .AddWithValue("Lab_Other_Result", VerifyStringNull(imLabRefSec.Lab_Other_Result))

                    If imLabRefSec.VisionText IsNot Nothing Then .AddWithValue("VisionText", VerifyStringNull(imLabRefSec.VisionText))
                    If imLabRefSec.HearingText IsNot Nothing Then .AddWithValue("HearingText", VerifyStringNull(imLabRefSec.HearingText))

                    If imLabRefSec.Referrals_NA IsNot Nothing Then .AddWithValue("Referrals_NA", VerifyBooleanNull(imLabRefSec.Referrals_NA))
                    If imLabRefSec.Referrals_WIC IsNot Nothing Then .AddWithValue("Referrals_WIC", VerifyBooleanNull(imLabRefSec.Referrals_WIC))
                    If imLabRefSec.Referrals_PhysicalTherapy IsNot Nothing Then .AddWithValue("Referrals_PhysicalTherapy", VerifyBooleanNull(imLabRefSec.Referrals_PhysicalTherapy))
                    If imLabRefSec.Referrals_OccupationTherapy IsNot Nothing Then .AddWithValue("Referrals_OccupationTherapy", VerifyBooleanNull(imLabRefSec.Referrals_OccupationTherapy))
                    If imLabRefSec.Referrals_SpeechTherapy IsNot Nothing Then .AddWithValue("Referrals_SpeechTherapy", VerifyBooleanNull(imLabRefSec.Referrals_SpeechTherapy))
                    If imLabRefSec.Referrals_Audiology IsNot Nothing Then .AddWithValue("Referrals_Audiology", VerifyBooleanNull(imLabRefSec.Referrals_Audiology))
                    If imLabRefSec.Referrals_Dental IsNot Nothing Then .AddWithValue("Referrals_Dental", VerifyBooleanNull(imLabRefSec.Referrals_Dental))
                    If imLabRefSec.Referrals_BehavioralHealth IsNot Nothing Then .AddWithValue("Referrals_BehavioralHealth", VerifyBooleanNull(imLabRefSec.Referrals_BehavioralHealth))
                    If imLabRefSec.Referrals_EarlyIntervention IsNot Nothing Then .AddWithValue("Referrals_EarlyIntervention", VerifyBooleanNull(imLabRefSec.Referrals_EarlyIntervention))
                    If imLabRefSec.Referrals_MentalHealthSpecialist IsNot Nothing Then .AddWithValue("Referrals_MentalHealthSpecialist", VerifyBooleanNull(imLabRefSec.Referrals_MentalHealthSpecialist))
                    If imLabRefSec.Referrals_Nutritionist IsNot Nothing Then .AddWithValue("Referrals_Nutritionist", VerifyBooleanNull(imLabRefSec.Referrals_Nutritionist))
                    If imLabRefSec.Referrals_Optometrist IsNot Nothing Then .AddWithValue("Referrals_Optometrist", VerifyBooleanNull(imLabRefSec.Referrals_Optometrist))
                    If imLabRefSec.Referrals_Ophthalmology IsNot Nothing Then .AddWithValue("Referrals_Ophthalmology", VerifyBooleanNull(imLabRefSec.Referrals_Ophthalmology))
                    If imLabRefSec.Referrals_OtherSpecialtyText IsNot Nothing Then .AddWithValue("Referrals_OtherSpecialtyText", VerifyStringNull(imLabRefSec.Referrals_OtherSpecialtyText))

                    If imLabRefSec.Immuno_Others_Checkbox IsNot Nothing Then .AddWithValue("Immuno_Others_Checkbox", VerifyBooleanNull(imLabRefSec.Immuno_Others_Checkbox))
                    If imLabRefSec.Labs_Others_Checkbox IsNot Nothing Then .AddWithValue("Labs_Others_Checkbox", VerifyBooleanNull(imLabRefSec.Labs_Others_Checkbox))
                    If imLabRefSec.Referals_Others_Checkbox IsNot Nothing Then .AddWithValue("Referals_Others_Checkbox", VerifyBooleanNull(imLabRefSec.Referals_Others_Checkbox))

                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveAdvanceDirectivesSection(ByVal claimKey As Long, ByVal AdvDirectiveSec As AdvanceDirectiveSection,
                                             ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If AdvDirectiveSec IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveAdvanceDirectives"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("RefuseToCompleteAdv", VerifyBooleanNull(AdvDirectiveSec.RefuseToCompleteAdvance))
                    .AddWithValue("AdvCarePlanDiscussed", VerifyBooleanNull(AdvDirectiveSec.AdvanceCarePlanDiscussed))
                    .AddWithValue("AdvCarePlanExecuteOn", VerifyBooleanNull(AdvDirectiveSec.AdvanceCarePlanExecutedOnCheck))

                    If Not IsNothing(AdvDirectiveSec.AdvanceCarePlanExecuteOn) _
                        AndAlso Not IsNothing(AdvDirectiveSec.AdvanceCarePlanExecuteOn.Value) _
                        AndAlso AdvDirectiveSec.AdvanceCarePlanExecuteOn.Value.HasValue _
                        AndAlso IsDate(AdvDirectiveSec.AdvanceCarePlanExecuteOn.Value.Value) Then

                        If AdvDirectiveSec.AdvanceCarePlanExecuteOn.Value.Value < CDate("01/01/1753") _
                            OrElse AdvDirectiveSec.AdvanceCarePlanExecuteOn.Value.Value > CDate("12/31/2999") Then

                            AdvDirectiveSec.AdvanceCarePlanExecuteOn.Value = CDate("01/01/1753")

                        End If

                    End If

                    .AddWithValue("AdvCarePlanExecuteOnDate", VerifyDateNull(AdvDirectiveSec.AdvanceCarePlanExecuteOn))

                End With

                dbo.ExecuteCommand(cmd)
            End If

        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveReviewOfSystemSection(ByVal claimKey As Long, ByVal rosSec As ReviewOfSystemSection,
                                          ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If rosSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveReviewOfSystem"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("Constitutional", VerifyStringNull(rosSec.Constitutional))
                    .AddWithValue("HEENTOral", VerifyStringNull(rosSec.HEENTOral))
                    .AddWithValue("AllergicImmunologic", VerifyStringNull(rosSec.AllergicImmunologic))
                    .AddWithValue("HematologicLymphatic", VerifyStringNull(rosSec.HematologicLymphatic))
                    .AddWithValue("Cardiovascular", VerifyStringNull(rosSec.Cardiovascular))
                    .AddWithValue("Gastrointestinal", VerifyStringNull(rosSec.Gastrointestinal))
                    .AddWithValue("Genitourinary", VerifyStringNull(rosSec.Genitourinary))
                    .AddWithValue("Respiratory", VerifyStringNull(rosSec.Respiratory))
                    .AddWithValue("Musculoskeletal", VerifyStringNull(rosSec.Musculoskeletal))
                    .AddWithValue("Neurological", VerifyStringNull(rosSec.Neurological))
                    .AddWithValue("Endocrine", VerifyStringNull(rosSec.Endocrine))
                    .AddWithValue("Integumentary", VerifyStringNull(rosSec.Integumentary))
                    .AddWithValue("Psychiatric", VerifyStringNull(rosSec.Psychiatric))
                    .AddWithValue("UrinaryIncontinence", VerifyBooleanNull(rosSec.UrinaryIncontinenceLeaking))
                    .AddWithValue("UrinaryIncontinence_BladderExercises", VerifyBooleanNull(rosSec.UrinaryIncontinence_BladderExercises))
                    .AddWithValue("UrinaryIncontinence_TreatmentWithMedicine", VerifyBooleanNull(rosSec.UrinaryIncontinence_TreatmentWithMedicine))
                    .AddWithValue("UrinaryIncontinence_SurgicalIntervention", VerifyBooleanNull(rosSec.UrinaryIncontinence_SurgicalIntervention))
                    .AddWithValue("PositiveNotes", VerifyStringNull(rosSec.DescribePositiveROS))
                    .AddWithValue("UrinaryIncontinence_Other", VerifyStringNull(rosSec.UrinaryIncontinence_Other))
                    .AddWithValue("UrinaryIncontinence_CheckBoxOther", VerifyBooleanNull(rosSec.UrinaryIncontinence_CheckBoxOther))
                    .AddWithValue("HearingDifficulty", VerifyIntegerNull(rosSec.HearingDifficulty))
                    'TODO  ADD '




                End With

                dbo.ExecuteCommand(cmd)

            End If

        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveMedicationListSection(ByVal claimKey As Long, ByVal medListSec As MedicationListSection,
                                          ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If medListSec IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMedicationList2020"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("PatientCurrentlyNoUse", VerifyBooleanNull(medListSec.CurrentlyDoesNotUse))
                    '.AddWithValue("CurrentMedicationList", medListSec.CurrentMedication)
                    '.AddWithValue("AdherenceMedicationList", medListSec.AdherenceMedicationList)
                    '.AddWithValue("RecentMedicationList", VerifyStringNull(medListSec.RecentMedication))

                    If IsNothing(medListSec.CurrentMedication) Then
                        medListSec.CurrentMedication = New List(Of MedicationItem)
                    End If

                    If IsNothing(medListSec.AdherenceMedicationList) Then
                        medListSec.AdherenceMedicationList = New List(Of MedicationItem)
                    End If

                    If IsNothing(medListSec.AllergiesMedicationList) Then
                        medListSec.AllergiesMedicationList = New List(Of MedicationItem)
                    End If

                    If (medListSec?.CurrentMedication IsNot Nothing AndAlso medListSec?.CurrentMedication?.Count > 0) _
                        OrElse (medListSec?.AdherenceMedicationList IsNot Nothing AndAlso medListSec?.AdherenceMedicationList?.Count > 0) _
                        OrElse (medListSec?.AllergiesMedicationList IsNot Nothing AndAlso medListSec?.AllergiesMedicationList?.Count > 0) Then

                        Dim MedListTable As New DataTable

                        MedListTable.Columns.Add("MedicationName", Type.GetType("System.String"))
                        MedListTable.Columns.Add("isAdherence", Type.GetType("System.Boolean"))
                        MedListTable.Columns.Add("isHistoric", Type.GetType("System.Boolean"))
                        MedListTable.Columns.Add("isConfirmed", Type.GetType("System.Boolean"))

                        For Each m In medListSec.CurrentMedication

                            Dim drMed As DataRow = MedListTable.NewRow

                            drMed("MedicationName") = m.MedicationName.Trim.ToString
                            drMed("isAdherence") = False
                            drMed("isHistoric") = m.isHistoric
                            drMed("isConfirmed") = m.isConfirmed

                            MedListTable.Rows.Add(drMed)

                        Next


                        If _isGHP AndAlso _memberAge < 21 Then
                            For Each m In medListSec.AllergiesMedicationList

                                Dim drMed As DataRow = MedListTable.NewRow

                                drMed("MedicationName") = m.MedicationName.Trim.ToString
                                drMed("IsAdherence") = True
                                drMed("isHistoric") = m.isHistoric
                                drMed("isConfirmed") = m.isConfirmed

                                MedListTable.Rows.Add(drMed)

                            Next
                        Else
                            For Each m In medListSec.AdherenceMedicationList

                                Dim drMed As DataRow = MedListTable.NewRow

                                drMed("MedicationName") = m.MedicationName.Trim.ToString
                                drMed("IsAdherence") = True
                                drMed("isHistoric") = m.isHistoric
                                drMed("isConfirmed") = m.isConfirmed

                                MedListTable.Rows.Add(drMed)

                            Next
                        End If

                        Dim param As New SqlParameter("MedList", SqlDbType.Structured)
                        param.Value = MedListTable

                        .Add(param)

                    End If


                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveAllergiesMedicationListSection(ByVal claimKey As Long, ByVal medListSec As MedicationListSection,
                                          ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If medListSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveAlergMedicationList2020"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NotKnowAllergies", VerifyBooleanNull(medListSec.NotKnowAllergies))

                    Dim MedListTable As New DataTable

                    MedListTable.Columns.Add("MedicationName", Type.GetType("System.String"))
                    MedListTable.Columns.Add("IsAdherence", Type.GetType("System.Boolean"))
                    MedListTable.Columns.Add("isHistoric", Type.GetType("System.Boolean"))
                    MedListTable.Columns.Add("isConfirmed", Type.GetType("System.Boolean"))


                    For Each m In medListSec.AllergiesMedicationList

                        Dim drMed As DataRow = MedListTable.NewRow

                        drMed("MedicationName") = m.MedicationName.Trim.ToString
                        drMed("IsAdherence") = False
                        drMed("isHistoric") = m.isHistoric
                        drMed("isConfirmed") = m.isConfirmed


                        MedListTable.Rows.Add(drMed)

                    Next

                    Dim param As New SqlParameter("MedList", SqlDbType.Structured)
                    param.Value = MedListTable

                    .Add(param)

                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

#End Region

#Region " Page 2 Save Section"

    Private Sub SaveMedicationReviewPatientCaregiverSection(ByVal claimKey As Long, ByVal mrpcSec As MedicalReviewSection,
                                                            ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If mrpcSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMedicationReview"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)

                    If mrpcSec.Question1 IsNot Nothing Then .AddWithValue("Question1", VerifyBooleanNull(mrpcSec.Question1))
                    If mrpcSec.Question2 IsNot Nothing Then .AddWithValue("Question2", VerifyBooleanNull(mrpcSec.Question2))
                    If mrpcSec.Question3 IsNot Nothing Then .AddWithValue("Question3", VerifyBooleanNull(mrpcSec.Question3))

                End With

                dbo.ExecuteCommand(cmd)
            Else
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMedicationReview"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveCognitiveAssessmentSection(ByVal claimKey As Long, ByVal caSec As CognitiveAssesmentSection,
                                               ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If caSec IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveCognitiveAssessment"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    '.AddWithValue("OrientedInTime", VerifyBooleanNull(caSec.Time.Value))
                    '.AddWithValue("OrientedInPlace", VerifyBooleanNull(caSec.Place.Value))
                    '.AddWithValue("OrientedInPerson", VerifyBooleanNull(caSec.Person.Value))
                    If caSec.DayOfTheWeek IsNot Nothing Then .AddWithValue("DayOfTheWeek", VerifyBooleanNull(caSec.DayOfTheWeek))
                    If caSec.MonthOfTheYear IsNot Nothing Then .AddWithValue("MonthOfTheYear", VerifyBooleanNull(caSec.MonthOfTheYear))
                    If caSec.Year IsNot Nothing Then .AddWithValue("Year", VerifyBooleanNull(caSec.Year))
                    If caSec.Ball IsNot Nothing Then .AddWithValue("Ball", VerifyBooleanNull(caSec.Ball))
                    If caSec.Flag IsNot Nothing Then .AddWithValue("Flag", VerifyBooleanNull(caSec.Flag))
                    If caSec.Tree IsNot Nothing Then .AddWithValue("Tree", VerifyBooleanNull(caSec.Tree))
                    If caSec.WNL IsNot Nothing Then .AddWithValue("WNL", VerifyBooleanNull(caSec.WNL))
                    If caSec.Diagnosis IsNot Nothing Then .AddWithValue("Dx", VerifyStringNull(caSec.Diagnosis))
                    If caSec.PlanGoalsTreatmentInterventionFollowUp IsNot Nothing Then .AddWithValue("PlanOf", VerifyStringNull(caSec.PlanGoalsTreatmentInterventionFollowUp))

                End With

                dbo.ExecuteCommand(cmd)
            Else
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveCognitiveAssessment"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                End With

                dbo.ExecuteCommand(cmd)


            End If


        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SavePainScreeningSection(ByVal claimKey As Long, ByVal psSec As PainScreeningSection,
                                         ByVal dbo As SqlDataObject)

        'TODO: PainScreeningSection
        Dim cmd As New SqlClient.SqlCommand

        Try
            If psSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSavePainScreening"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    If psSec.PatientHaveComplaint IsNot Nothing Then .AddWithValue("PatientHaveComplaint", VerifyBooleanNull(psSec.PatientHaveComplaint))
                    If psSec.PainIsLocated IsNot Nothing Then .AddWithValue("PainLocation", VerifyStringNull(psSec.PainIsLocated))
                    If psSec.ManagePainWith IsNot Nothing Then .AddWithValue("TreatmentOrMedication", VerifyStringNull(psSec.ManagePainWith))
                    If psSec.TreatmentHaveBeenEffectiveNA IsNot Nothing Then .AddWithValue("TreatmentHaveBeenEffectiveNA", VerifyBooleanNull(psSec.TreatmentHaveBeenEffectiveNA))
                    If psSec.TreatmentHaveBeenEffective IsNot Nothing Then .AddWithValue("MedicationEffective", VerifyBooleanNull(psSec.TreatmentHaveBeenEffective))
                    If psSec.RatePainExperiencingNow IsNot Nothing Then .AddWithValue("PainRate", VerifyNull(psSec.RatePainExperiencingNow.Value))
                    If psSec.Transportation IsNot Nothing Then .AddWithValue("Transportation", VerifyBooleanNull(psSec.Transportation))
                    If psSec.BathingDressing IsNot Nothing Then .AddWithValue("BathingDressing", VerifyBooleanNull(psSec.BathingDressing))
                    If psSec.WalkingAbility IsNot Nothing Then .AddWithValue("WalkingAbility", VerifyBooleanNull(psSec.WalkingAbility))
                    If psSec.EnjoymentOfLife IsNot Nothing Then .AddWithValue("EnjoymentOfLife", VerifyBooleanNull(psSec.EnjoymentOfLife))
                    If psSec.Toileting IsNot Nothing Then .AddWithValue("Toileting", VerifyBooleanNull(psSec.Toileting))
                    If psSec.Sleep IsNot Nothing Then .AddWithValue("Sleep", VerifyBooleanNull(psSec.Sleep))
                    If psSec.Mood IsNot Nothing Then .AddWithValue("Mood", VerifyBooleanNull(psSec.Mood))
                    If psSec.NA IsNot Nothing Then .AddWithValue("NA", VerifyBooleanNull(psSec.NA))
                    If psSec.Employment IsNot Nothing Then .AddWithValue("Employment", VerifyBooleanNull(psSec.Employment))
                    If psSec.HouseWork IsNot Nothing Then .AddWithValue("Housework", VerifyBooleanNull(psSec.HouseWork))
                    If psSec.FoodPreparation IsNot Nothing Then .AddWithValue("FoodPreparation", VerifyBooleanNull(psSec.FoodPreparation))
                    If psSec.RelationshipWithOther IsNot Nothing Then .AddWithValue("Relationships", VerifyBooleanNull(psSec.RelationshipWithOther))
                    If psSec.PainDueTo IsNot Nothing Then .AddWithValue("PainDueTo", VerifyStringNull(psSec.PainDueTo))
                    If psSec.PlanGoalsTreatmentInterventaionFollowUp IsNot Nothing Then .AddWithValue("PlanOf", VerifyStringNull(psSec.PlanGoalsTreatmentInterventaionFollowUp))
                    If psSec.ArthritisDueToInfection IsNot Nothing Then .AddWithValue("ArthritisDueInfection", VerifyBooleanNull(psSec.ArthritisDueToInfection))

                    If psSec.Others IsNot Nothing Then
                        .AddWithValue("Others", VerifyStringNull(psSec.Others))
                        psSec.PainEvaluationOtherCondition = New BooleanField
                        psSec.PainEvaluationOtherCondition.Value = VerifyBooleanNull(True)
                    End If

                    If psSec.PainEvaluationOtherActivities Is Nothing Then
                        psSec.PainEvaluationOtherActivities = New BooleanField
                        psSec.PainEvaluationOtherActivities.Value = False
                    End If

                    If psSec.PainEvaluationOtherCondition IsNot Nothing Then .AddWithValue("PainEvaluationOtherCondition", VerifyBooleanNull(psSec.PainEvaluationOtherCondition))
                    If psSec.PainEvaluationOtherConditionText IsNot Nothing Then .AddWithValue("PainEvaluationOtherConditionText", VerifyStringNull(psSec.Others))
                    If psSec.PainEvaluationOtherActivities IsNot Nothing Then .AddWithValue("PainEvaluation_OtherActivities", VerifyBooleanNull(psSec.PainEvaluationOtherActivities))
                    ' .AddWithValue("CausalCondition", VerifyStringNull(psSec.CausalCondition.Value))
                End With

                dbo.ExecuteCommand(cmd)
            End If

        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveActivitiesDailyLiving(ByVal claimKey As Long, ByVal acdlSec As ActivitiesOfDailyLivingSection,
                                          ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If acdlSec IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveActivitiesDailyLiving"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    If acdlSec.Bathing IsNot Nothing Then .AddWithValue("Bathing", VerifyBooleanNull(acdlSec.Bathing))
                    If acdlSec.BathingComments IsNot Nothing Then .AddWithValue("BathingComments", VerifyStringNull(acdlSec.BathingComments))
                    If acdlSec.DressingAndUndressing IsNot Nothing Then .AddWithValue("DressingUnDressing", VerifyBooleanNull(acdlSec.DressingAndUndressing))
                    If acdlSec.DressingAndUndressingComments IsNot Nothing Then .AddWithValue("DressingUnDressingComments", VerifyStringNull(acdlSec.DressingAndUndressingComments))
                    If acdlSec.Eating IsNot Nothing Then .AddWithValue("Eating", VerifyBooleanNull(acdlSec.Eating))
                    If acdlSec.EatingComments IsNot Nothing Then .AddWithValue("EatingComments", VerifyStringNull(acdlSec.EatingComments))
                    If acdlSec.TransferringBedChair IsNot Nothing Then .AddWithValue("TransferingFrom", VerifyBooleanNull(acdlSec.TransferringBedChair))
                    If acdlSec.TransferringBedChairComments IsNot Nothing Then .AddWithValue("TransferingFromComments", VerifyStringNull(acdlSec.TransferringBedChairComments))
                    If acdlSec.VoluntarilyControl IsNot Nothing Then .AddWithValue("VoluntarilyControl", VerifyBooleanNull(acdlSec.VoluntarilyControl))
                    If acdlSec.VoluntarilyControlComments IsNot Nothing Then .AddWithValue("VoluntarilyControlComments", VerifyStringNull(acdlSec.VoluntarilyControlComments))
                    If acdlSec.UsingToilet IsNot Nothing Then .AddWithValue("UsingToilet", VerifyBooleanNull(acdlSec.UsingToilet))
                    If acdlSec.UsingToiletComments IsNot Nothing Then .AddWithValue("UsingToiletComments", VerifyStringNull(acdlSec.UsingToiletComments))
                    If acdlSec.Walking IsNot Nothing Then .AddWithValue("Walking", VerifyBooleanNull(acdlSec.Walking))
                    If acdlSec.WalkingComments IsNot Nothing Then .AddWithValue("WalkingComments", VerifyStringNull(acdlSec.WalkingComments))
                    If acdlSec.BedFast IsNot Nothing Then .AddWithValue("BedFast", VerifyBooleanNull(acdlSec.BedFast))
                    If acdlSec.HistoryOfFalling IsNot Nothing Then .AddWithValue("HistoryOfFalling", VerifyBooleanNull(acdlSec.HistoryOfFalling))
                    If acdlSec.HistoryOfFalling_Comments IsNot Nothing Then .AddWithValue("HistoryOfFalling_Comments", VerifyStringNull(acdlSec.HistoryOfFalling_Comments))

                    If acdlSec.DependenceOnOxygen IsNot Nothing Then .AddWithValue("DependenceOnOxygen", VerifyBooleanNull(acdlSec.DependenceOnOxygen))
                    If acdlSec.DependenceOnRespirator IsNot Nothing Then .AddWithValue("DependenceOnRespirator", VerifyBooleanNull(acdlSec.DependenceOnRespirator))
                    If acdlSec.DependenceOnWheelchair IsNot Nothing Then .AddWithValue("DependenceOnWheelchair", VerifyBooleanNull(acdlSec.DependenceOnWheelchair))
                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

#End Region

#Region " Page 3 Save Section"

    Private Sub SaveScreeningTest(ByVal claimKey As Long, ByVal stSec As ScreeningScheduleSection,
                                  ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If stSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveScreeningTest"
                'cmd.CommandText = "uspSaveScreeningTest"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)

                    If stSec.BoneMineralDensityDate IsNot Nothing Then

                        If stSec.BoneMineralDensityDate.Value IsNot Nothing AndAlso
                            stSec.BoneMineralDensityDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.BoneMineralDensityDate.Value.Value) Then

                            stSec.BoneMineralDensityDate.Value = Nothing

                        End If

                        .AddWithValue("BMDDate", VerifyDateNull(stSec.BoneMineralDensityDate))

                    End If

                    'AHA 2020 Changes'
                    If stSec.BoneMineralDensityResult_Normal IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Normal", VerifyBooleanNull(stSec.BoneMineralDensityResult_Normal))
                    If stSec.BoneMineralDensityResult_Osteopenia IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Osteopenia", VerifyBooleanNull(stSec.BoneMineralDensityResult_Osteopenia))
                    If stSec.BoneMineralDensityResult_Osteoporosis IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Osteoporosis", VerifyBooleanNull(stSec.BoneMineralDensityResult_Osteoporosis))
                    If stSec.BoneMineralDensityResult_NA IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_NA", VerifyBooleanNull(stSec.BoneMineralDensityResult_NA))
                    If stSec.BoneMineralDensityResult_Other IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Other", VerifyBooleanNull(stSec.BoneMineralDensityResult_Other))



                    If stSec.BoneMineralDensityResult IsNot Nothing Then .AddWithValue("BMDResult", VerifyStringNull(stSec.BoneMineralDensityResult))
                    '.AddWithValue("BMDReviewed", VerifyBooleanNull(stSec.BoneMineralDensityReviewed))
                    If stSec.BoneMineralDensityNAFor IsNot Nothing Then .AddWithValue("BoneMineralDensity_NAFor", VerifyStringNull(stSec.BoneMineralDensityNAFor))
                    If stSec.BoneMineralDensityPrescribed IsNot Nothing Then .AddWithValue("BMDPrescribed", VerifyBooleanNull(stSec.BoneMineralDensityPrescribed))
                    If stSec.BoneMineralDensityRxOrdered IsNot Nothing Then .AddWithValue("BMDRxOrdered", VerifyBooleanNull(stSec.BoneMineralDensityRxOrdered))

                    If stSec.CardiovascularLDLDate IsNot Nothing Then

                        If stSec.CardiovascularLDLDate.Value IsNot Nothing AndAlso
                            stSec.CardiovascularLDLDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.CardiovascularLDLDate.Value.Value) Then

                            stSec.CardiovascularLDLDate.Value = Nothing

                        End If

                        .AddWithValue("CardiovascularLDLDate", VerifyDateNull(stSec.CardiovascularLDLDate))
                    End If
                    If stSec.CardiovascularLDLResult IsNot Nothing Then .AddWithValue("CardiovascularLDLResult", VerifyStringNull(stSec.CardiovascularLDLResult))
                    '.AddWithValue("CardiovascularLDLReviewed", VerifyBooleanNull(stSec.CardiovascularLDLReviewed))
                    If stSec.CardiovascularLDLNAFor IsNot Nothing Then .AddWithValue("Screening_Cardio_LDL_NAFor", VerifyStringNull(stSec.CardiovascularLDLNAFor))
                    If stSec.CardiovascularLDLPrescribed IsNot Nothing Then .AddWithValue("CardiovascularLDLPrescribed", VerifyBooleanNull(stSec.CardiovascularLDLPrescribed))

                    If stSec.CardiovascularBetaDate IsNot Nothing Then

                        If stSec.CardiovascularBetaDate.Value IsNot Nothing AndAlso
                            stSec.CardiovascularBetaDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.CardiovascularBetaDate.Value.Value) Then

                            stSec.CardiovascularBetaDate.Value = Nothing

                        End If

                        .AddWithValue("CardiovascularBetaDate", VerifyDateNull(stSec.CardiovascularBetaDate))
                    End If

                    If stSec.CardiovascularBetaResult IsNot Nothing Then .AddWithValue("CardiovascularBetaResult", VerifyStringNull(stSec.CardiovascularBetaResult))
                    '.AddWithValue("CardiovascularBetaReviewed", VerifyBooleanNull(stSec.CardiovascularBetaReviewed))
                    If stSec.CardiovascularBetaNAFor IsNot Nothing Then .AddWithValue("Screening_Cardio_BetaBlocke_NAFor", VerifyStringNull(stSec.CardiovascularBetaNAFor))
                    If stSec.CardiovascularBetaPrescribed IsNot Nothing Then .AddWithValue("CardiovascularBetaPrescribed", VerifyNull(stSec.CardiovascularBetaPrescribed.Value))

                    If stSec.ColorectalCancerScreeningSelectedIndex IsNot Nothing Then .AddWithValue("ColorectalCancerSelectedIndex", VerifyNull(stSec.ColorectalCancerScreeningSelectedIndex.Value))

                    If stSec.ColorectalCancerScreeningDate IsNot Nothing Then

                        If stSec.ColorectalCancerScreeningDate.Value IsNot Nothing AndAlso stSec.ColorectalCancerScreeningDate.Value.HasValue AndAlso Not Globals.ValidateDateMinMaxRange(stSec.ColorectalCancerScreeningDate.Value.Value) Then
                            stSec.ColorectalCancerScreeningDate.Value = Nothing
                        End If

                        .AddWithValue("ColorectalCancerDoneDate", VerifyDateNull(stSec.ColorectalCancerScreeningDate))
                    End If

                    If stSec.ColorectalCancerScreeningResult IsNot Nothing Then .AddWithValue("ColorectalCancerResult", VerifyStringNull(stSec.ColorectalCancerScreeningResult))
                    '.AddWithValue("ColorectalCancerReviewed", VerifyBooleanNull(stSec.ColorectalCancerScreeningReviewed))
                    If stSec.ColorectalCancerScreeningNAFor IsNot Nothing Then .AddWithValue("Screening_ColorectalCancer_NAFor", VerifyStringNull(stSec.ColorectalCancerScreeningNAFor))
                    If stSec.ColorectalCancerScreeningPrescribed IsNot Nothing Then .AddWithValue("ColorectalCancerPrescribed", VerifyBooleanNull(stSec.ColorectalCancerScreeningPrescribed))

                    If stSec.IsDiabetic IsNot Nothing Then .AddWithValue("IsDiabetic", VerifyBooleanNull(stSec.IsDiabetic))

                    If stSec.DiabetesScreening_DilatedEyeExamDate IsNot Nothing Then

                        If stSec.DiabetesScreening_DilatedEyeExamDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_DilatedEyeExamDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_DilatedEyeExamDate.Value.Value) Then

                            stSec.DiabetesScreening_DilatedEyeExamDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesDilatedEyeDate", VerifyDateNull(stSec.DiabetesScreening_DilatedEyeExamDate))
                    End If

                    If stSec.DiabetesScreening_DilatedEyeExamResult IsNot Nothing Then .AddWithValue("DiabetesDilatedEyeResult", VerifyStringNull(stSec.DiabetesScreening_DilatedEyeExamResult))
                    '.AddWithValue("DiabetesDilatedEyeReviewed", VerifyBooleanNull(stSec.DiabetesScreening_DilatedEyeExamReviewed))
                    If stSec.DiabetesScreening_DilatedEyeExamNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_DilatedEye_NAFor", VerifyStringNull(stSec.DiabetesScreening_DilatedEyeExamNAFor))
                    If stSec.DiabetesScreening_DilatedEyeExamPrescribed IsNot Nothing Then .AddWithValue("DiabetesDilatedEyePrescribed", VerifyBooleanNull(stSec.DiabetesScreening_DilatedEyeExamPrescribed))

                    If stSec.DiabetesScreening_LDLDate IsNot Nothing Then

                        If stSec.DiabetesScreening_LDLDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_LDLDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_LDLDate.Value.Value) Then

                            stSec.DiabetesScreening_LDLDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesLDLDate", VerifyDateNull(stSec.DiabetesScreening_LDLDate))
                    End If


                    If stSec.DiabetesScreening_LDLResult IsNot Nothing Then .AddWithValue("DiabetesLDLResult", VerifyStringNull(stSec.DiabetesScreening_LDLResult))
                    '.AddWithValue("DiabetesLDLReviewed", VerifyBooleanNull(stSec.DiabetesScreening_LDLReviewed))
                    If stSec.DiabetesScreening_LDLNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_LDL_NAFor", VerifyStringNull(stSec.DiabetesScreening_LDLNAFor))
                    If stSec.DiabetesScreening_LDLPrescribed IsNot Nothing Then .AddWithValue("DiabetesLDLPrescribed", VerifyBooleanNull(stSec.DiabetesScreening_LDLPrescribed))

                    If stSec.DiabetesScreening_HGA1C_Date IsNot Nothing Then

                        If stSec.DiabetesScreening_HGA1C_Date.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_HGA1C_Date.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_HGA1C_Date.Value.Value) Then

                            stSec.DiabetesScreening_HGA1C_Date.Value = Nothing

                        End If

                        .AddWithValue("DiabetesScreening_HGA1C_Date", VerifyDateNull(stSec.DiabetesScreening_HGA1C_Date))
                    End If


                    If stSec.DiabetesScreening_HGA1C_Result IsNot Nothing Then .AddWithValue("DiabetesScreening_HGA1C_Result", VerifyStringNull(stSec.DiabetesScreening_HGA1C_Result))
                    If stSec.DiabetesScreening_HGA1C_NAFor IsNot Nothing Then .AddWithValue("DiabetesScreening_HGA1C_NAFor", VerifyStringNull(stSec.DiabetesScreening_HGA1C_NAFor))
                    If stSec.DiabetesScreening_HGA1C_Prescribed IsNot Nothing Then .AddWithValue("DiabetesScreening_HGA1C_Prescribed", VerifyBooleanNull(stSec.DiabetesScreening_HGA1C_Prescribed))

                    If stSec.DiabetesScreening_MicroalbuminDate IsNot Nothing Then

                        If stSec.DiabetesScreening_MicroalbuminDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_MicroalbuminDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_MicroalbuminDate.Value.Value) Then

                            stSec.DiabetesScreening_MicroalbuminDate.Value = Nothing

                        End If
                        .AddWithValue("DiabetesMicroalbuminDate", VerifyDateNull(stSec.DiabetesScreening_MicroalbuminDate))
                    End If


                    If stSec.DiabetesScreening_MicroalbuminResult IsNot Nothing Then .AddWithValue("DiabetesMicroalbuminResult", VerifyStringNull(stSec.DiabetesScreening_MicroalbuminResult))
                    '.AddWithValue("DiabetesMicroalbuminReviewed", VerifyBooleanNull(stSec.DiabetesScreening_MicroalbuminReviewed))
                    If stSec.DiabetesScreening_MicroalbuminNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_Microalbumin_NAFor", VerifyStringNull(stSec.DiabetesScreening_MicroalbuminNAFor))
                    If stSec.DiabetesScreening_MicroalbuminPrescribed IsNot Nothing Then .AddWithValue("DiabetesMicroalbuminPrescribed", VerifyBooleanNull(stSec.DiabetesScreening_MicroalbuminPrescribed))

                    If stSec.GlaucomaTestDate IsNot Nothing Then

                        If stSec.GlaucomaTestDate.Value IsNot Nothing AndAlso
                            stSec.GlaucomaTestDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.GlaucomaTestDate.Value.Value) Then

                            stSec.GlaucomaTestDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesGlaucomaTestDate", VerifyDateNull(stSec.GlaucomaTestDate))
                    End If


                    If stSec.GlaucomaTestResult IsNot Nothing Then .AddWithValue("DiabetesGlaucomaResult", VerifyStringNull(stSec.GlaucomaTestResult))
                    '.AddWithValue("DiabetesGlaucomaReviewed", VerifyBooleanNull(stSec.GlaucomaTestReviewed))
                    If stSec.GlaucomaTestNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_GlaucomaTest_NAFor", VerifyStringNull(stSec.GlaucomaTestNAFor))
                    If stSec.GlaucomaTestPrescribed IsNot Nothing Then .AddWithValue("DiabetesGlaucomaPrescribed", VerifyBooleanNull(stSec.GlaucomaTestPrescribed))

                    If stSec.Mammogram_ProstateCancerDate IsNot Nothing Then

                        If stSec.Mammogram_ProstateCancerDate.Value IsNot Nothing AndAlso
                            stSec.Mammogram_ProstateCancerDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.Mammogram_ProstateCancerDate.Value.Value) Then

                            stSec.Mammogram_ProstateCancerDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesMammogramProstateDate", VerifyDateNull(stSec.Mammogram_ProstateCancerDate))
                    End If

                    If stSec.Mammogram_ProstateCancerResult IsNot Nothing Then .AddWithValue("DiabetesMammogramProstateResult", VerifyStringNull(stSec.Mammogram_ProstateCancerResult))
                    '.AddWithValue("DiabetesMammogramProstateReviewed", VerifyBooleanNull(stSec.Mammogram_ProstateCancerReviewed))
                    If stSec.Mammogram_ProstateCancerNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_MammogramProstate_NAFor", VerifyStringNull(stSec.Mammogram_ProstateCancerNAFor))
                    If stSec.Mammogram_ProstateCancerPrescribed IsNot Nothing Then .AddWithValue("DiabetesMammogramProstatePrescribe", VerifyBooleanNull(stSec.Mammogram_ProstateCancerPrescribed))

                    If stSec.FluShotDate IsNot Nothing Then

                        If stSec.FluShotDate.Value IsNot Nothing AndAlso
                            stSec.FluShotDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.FluShotDate.Value.Value) Then

                            stSec.FluShotDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesFluShotDate", VerifyDateNull(stSec.FluShotDate))
                    End If


                    If stSec.FluShotComments IsNot Nothing Then .AddWithValue("DiabetesFluShotComments", VerifyStringNull(stSec.FluShotComments))
                    If stSec.FluShotPrescribed IsNot Nothing Then .AddWithValue("DiabetesFluShotPrescribed", VerifyBooleanNull(stSec.FluShotPrescribed))
                    'AHA 2020 Changes'
                    If stSec.FluShot_PatientRefuses IsNot Nothing Then .AddWithValue("DiabetesFluShotPatientRefuses", VerifyBooleanNull(stSec.FluShot_PatientRefuses))

                    If stSec.PneumococcalShotDate IsNot Nothing Then

                        If stSec.PneumococcalShotDate.Value IsNot Nothing AndAlso
                            stSec.PneumococcalShotDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.PneumococcalShotDate.Value.Value) Then

                            stSec.PneumococcalShotDate.Value = Nothing

                        End If
                        .AddWithValue("DiabetesPneumococcalShotDate", VerifyDateNull(stSec.PneumococcalShotDate))
                    End If


                    If stSec.PneumococcalShotComments IsNot Nothing Then .AddWithValue("DiabetesPneumococcalComments", VerifyStringNull(stSec.PneumococcalShotComments))
                    If stSec.PneumococcalShotPrescribed IsNot Nothing Then .AddWithValue("DiabetesPneumococcalPrescribed", VerifyBooleanNull(stSec.PneumococcalShotPrescribed))
                    'AHA 2020 Changes'
                    If stSec.PneumococcalShot_PatientRefuses IsNot Nothing Then .AddWithValue("DiabetesPneumococcalPatientRefuses", VerifyBooleanNull(stSec.PneumococcalShot_PatientRefuses))



                    'AHA 2022 Changes
                    If stSec.COVID19VaccineHouse IsNot Nothing Then .AddWithValue("COVID19VaccineHouse", VerifyStringNull(stSec.COVID19VaccineHouse))
                    If stSec.COVID19VaccineShot IsNot Nothing Then .AddWithValue("COVID19VaccineShot", VerifyIntegerNull(stSec.COVID19VaccineShot))

                    If stSec.COVID19VaccineRefuse IsNot Nothing Then .AddWithValue("COVID19VaccineRefuse", VerifyBooleanNull(stSec.COVID19VaccineRefuse))
                    If stSec.COVID19VaccineOrdered IsNot Nothing Then .AddWithValue("COVID19VaccineOrdered", VerifyBooleanNull(stSec.COVID19VaccineOrdered))



                    If stSec.COVID19VaccineShotDate1 IsNot Nothing Then

                        If stSec.COVID19VaccineShotDate1.Value IsNot Nothing AndAlso
                            stSec.COVID19VaccineShotDate1.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.COVID19VaccineShotDate1.Value.Value) Then

                            stSec.COVID19VaccineShotDate1.Value = Nothing

                        End If

                        .AddWithValue("COVID19VaccineShotDate1", VerifyDateNull(stSec.COVID19VaccineShotDate1))
                    End If

                    If stSec.COVID19VaccineShotDate2 IsNot Nothing Then

                        If stSec.COVID19VaccineShotDate2.Value IsNot Nothing AndAlso
                            stSec.COVID19VaccineShotDate2.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.COVID19VaccineShotDate2.Value.Value) Then

                            stSec.COVID19VaccineShotDate2.Value = Nothing

                        End If

                        .AddWithValue("COVID19VaccineShotDate2", VerifyDateNull(stSec.COVID19VaccineShotDate2))
                    End If

                    If stSec.COVID19VaccineShotDate3 IsNot Nothing Then

                        If stSec.COVID19VaccineShotDate3.Value IsNot Nothing AndAlso
                            stSec.COVID19VaccineShotDate3.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.COVID19VaccineShotDate3.Value.Value) Then

                            stSec.COVID19VaccineShotDate3.Value = Nothing

                        End If

                        .AddWithValue("COVID19VaccineShotDate3", VerifyDateNull(stSec.COVID19VaccineShotDate3))
                    End If

                    If stSec.ColorectalColonoscopy IsNot Nothing Then .AddWithValue("ColorectalColonoscopy", VerifyBooleanNull(stSec.ColorectalColonoscopy))

                    If stSec.ColorectalColonoscopyDate IsNot Nothing Then

                        If stSec.ColorectalColonoscopyDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalColonoscopyDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalColonoscopyDate.Value.Value) Then

                            stSec.ColorectalColonoscopyDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalColonoscopyDate", VerifyDateNull(stSec.ColorectalColonoscopyDate))
                    End If

                    'TODO: Se maneja los datos de Colorectal
                    'AHA 2020 Changes'
                    If stSec.Colorectal_ColonoscopyResult_NA IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_NA", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_NA))
                    If stSec.Colorectal_ColonoscopyResult_Negative IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Negative", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Negative))
                    If stSec.Colorectal_ColonoscopyResult_Diverticles IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Diverticles", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Diverticles))
                    If stSec.Colorectal_ColonoscopyResult_BleedingAreas IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_BleedingAreas", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_BleedingAreas))
                    If stSec.Colorectal_ColonoscopyResult_CAInColon IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_CAInColon", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_CAInColon))
                    If stSec.Colorectal_ColonoscopyResult_CAInRectum IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_CAInRectum", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_CAInRectum))
                    If stSec.Colorectal_ColonoscopyResult_Colitis IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Colitis", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Colitis))
                    If stSec.Colorectal_ColonoscopyResult_UlcerativeOlitis IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_UlcerativeOlitis", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_UlcerativeOlitis))
                    If stSec.Colorectal_ColonoscopyResult_CrohnsDisease IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_CrohnsDisease", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_CrohnsDisease))
                    If stSec.Colorectal_ColonoscopyResult_Polyps IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Polyps", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Polyps))
                    If stSec.Colorectal_ColonoscopyResult_Other IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Other", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Other))

                    'TODO: Se añade el valor de los resultados al campo para reporte.
                    'AHA 2020 Changes'
                    Dim Result As StringField = New StringField
                    Result.Value = ""

                    If stSec.ColorectalColonoscopyResult IsNot Nothing Then
                        stSec.ColorectalColonoscopyResult = New StringField()

                        If stSec.Colorectal_ColonoscopyResult_Negative.Value Then
                            Result.Value += "Negativo "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Diverticles.Value Then
                            Result.Value += "Diverticulos "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_BleedingAreas.Value Then
                            Result.Value += "Areas de Sangrado "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_CAInColon.Value Then
                            Result.Value += "CA en Colon "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_CAInRectum.Value Then
                            Result.Value += "CA en Recto "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Colitis.Value Then
                            Result.Value += "Colitis "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_UlcerativeOlitis.Value Then
                            Result.Value += "Ulceras "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_CrohnsDisease.Value Then
                            Result.Value += "Crohns "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Polyps.Value Then
                            Result.Value += "Polipos "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Other.Value Then
                            Result.Value += "Otros "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If

                        stSec.ColorectalColonoscopyResult.Value = Result.Value
                    End If

                    If stSec.ColorectalColonoscopyNAFor IsNot Nothing Then .AddWithValue("ColorectalColonoscopyNAFor", VerifyStringNull(stSec.ColorectalColonoscopyNAFor))
                    If stSec.ColorectalColonoscopyPrescribe IsNot Nothing Then .AddWithValue("ColorectalColonoscopyPrescribe", VerifyBooleanNull(stSec.ColorectalColonoscopyPrescribe))

                    If stSec.ColorectalOccultBlood IsNot Nothing Then .AddWithValue("ColorectalOccultBlood", VerifyBooleanNull(stSec.ColorectalOccultBlood))

                    If stSec.ColorectalOccultBloodDate IsNot Nothing Then

                        If stSec.ColorectalOccultBloodDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalOccultBloodDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalOccultBloodDate.Value.Value) Then

                            stSec.ColorectalOccultBloodDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalOccultBloodDate", VerifyDateNull(stSec.ColorectalOccultBloodDate))
                    End If

                    If stSec.ColorectalOccultBloodResult IsNot Nothing Then .AddWithValue("ColorectalOccultBloodResult", VerifyStringNull(stSec.ColorectalOccultBloodResult))
                    If stSec.ColorectalOccultBloodNAFor IsNot Nothing Then .AddWithValue("ColorectalOccultBloodNAFor", VerifyStringNull(stSec.ColorectalOccultBloodNAFor))
                    If stSec.ColorectalOccultBloodPrescribe IsNot Nothing Then .AddWithValue("ColorectalOccultBloodPrescribe", VerifyBooleanNull(stSec.ColorectalOccultBloodPrescribe))

                    If stSec.ColorectalFlexibleSigmoidoscopy IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopy", VerifyBooleanNull(stSec.ColorectalFlexibleSigmoidoscopy))

                    If stSec.ColorectalFlexibleSigmoidoscopyDate IsNot Nothing Then

                        If stSec.ColorectalFlexibleSigmoidoscopyDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalFlexibleSigmoidoscopyDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalFlexibleSigmoidoscopyDate.Value.Value) Then

                            stSec.ColorectalFlexibleSigmoidoscopyDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalFlexibleSigmoidoscopyDate", VerifyDateNull(stSec.ColorectalFlexibleSigmoidoscopyDate))
                    End If

                    'AHA 2020 Changes'
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_NA IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_NA", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_NA))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_Negative IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_Negative", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_Negative))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_AnalFissure IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_AnalFissure", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_AnalFissure))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInRectum IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_CAInRectum", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInRectum))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_Diverticles IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_Diverticles", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_Diverticles))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection))


                    If stSec.ColorectalFlexibleSigmoidoscopyResult IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopyResult", VerifyStringNull(stSec.ColorectalFlexibleSigmoidoscopyResult))
                    If stSec.ColorectalFlexibleSigmoidoscopyNAFor IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopyNAFor", VerifyStringNull(stSec.ColorectalFlexibleSigmoidoscopyNAFor))
                    If stSec.ColorectalFlexibleSigmoidoscopyPrescribe IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopyPrescribe", VerifyBooleanNull(stSec.ColorectalFlexibleSigmoidoscopyPrescribe))

                    If stSec.ColorectalFITDNA IsNot Nothing Then .AddWithValue("ColorectalFITDNA", VerifyBooleanNull(stSec.ColorectalFITDNA))

                    If stSec.ColorectalFITDNADate IsNot Nothing Then

                        If stSec.ColorectalFITDNADate.Value IsNot Nothing AndAlso
                            stSec.ColorectalFITDNADate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalFITDNADate.Value.Value) Then

                            stSec.ColorectalFITDNADate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalFITDNADate", VerifyDateNull(stSec.ColorectalFITDNADate))
                    End If

                    If stSec.ColorectalFITDNAResult IsNot Nothing Then .AddWithValue("ColorectalFITDNAResult", VerifyStringNull(stSec.ColorectalFITDNAResult))
                    If stSec.ColorectalFITDNANAFor IsNot Nothing Then .AddWithValue("ColorectalFITDNANAFor", VerifyStringNull(stSec.ColorectalFITDNANAFor))
                    If stSec.ColorectalFITDNAPrescribe IsNot Nothing Then .AddWithValue("ColorectalFITDNAPrescribe", VerifyBooleanNull(stSec.ColorectalFITDNAPrescribe))

                    If stSec.ColorectalColonographyCT IsNot Nothing Then .AddWithValue("ColorectalColonographyCT", VerifyBooleanNull(stSec.ColorectalColonographyCT))

                    If stSec.ColorectalColonographyCTDate IsNot Nothing Then

                        If stSec.ColorectalColonographyCTDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalColonographyCTDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalColonographyCTDate.Value.Value) Then

                            stSec.ColorectalColonographyCTDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalColonographyCTDate", VerifyDateNull(stSec.ColorectalColonographyCTDate))
                    End If

                    If stSec.ColorectalColonographyCTResult IsNot Nothing Then .AddWithValue("ColorectalColonographyCTResult", VerifyStringNull(stSec.ColorectalColonographyCTResult))
                    If stSec.ColorectalColonographyCTNAFor IsNot Nothing Then .AddWithValue("ColorectalColonographyCTNAFor", VerifyStringNull(stSec.ColorectalColonographyCTNAFor))
                    If stSec.ColorectalColonographyCTPrescribe IsNot Nothing Then .AddWithValue("ColorectalColonographyCTPrescribe", VerifyBooleanNull(stSec.ColorectalColonographyCTPrescribe))

                    If stSec.MammogramCancerDate IsNot Nothing Then

                        If stSec.MammogramCancerDate.Value IsNot Nothing AndAlso
                            stSec.MammogramCancerDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.MammogramCancerDate.Value.Value) Then

                            stSec.MammogramCancerDate.Value = Nothing

                        End If

                        .AddWithValue("MammogramCancerDate", VerifyDateNull(stSec.MammogramCancerDate))
                    End If

                    'AHA 2020 Changes'
                    If stSec.Mammogram_CancerResult_Category_0 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_0", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_0))
                    If stSec.Mammogram_CancerResult_Category_1 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_1", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_1))
                    If stSec.Mammogram_CancerResult_Category_2 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_2", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_2))
                    If stSec.Mammogram_CancerResult_Category_3 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_3", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_3))
                    If stSec.Mammogram_CancerResult_Category_4 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_4", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_4))
                    If stSec.Mammogram_CancerResult_Category_5 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_5", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_5))
                    If stSec.Mammogram_CancerResult_Category_6 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_6", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_6))
                    If stSec.Mammogram_CancerResult_NA IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_NA", VerifyBooleanNull(stSec.Mammogram_CancerResult_NA))

                    If stSec.MammogramCancerResult IsNot Nothing Then .AddWithValue("MammogramCancerResult", VerifyStringNull(stSec.MammogramCancerResult))
                    If stSec.MammogramCancerNAFor IsNot Nothing Then .AddWithValue("MammogramCancerNAFor", VerifyStringNull(stSec.MammogramCancerNAFor))
                    If stSec.MammogramCancerPrescribed IsNot Nothing Then .AddWithValue("MammogramCancerPrescribed", VerifyBooleanNull(stSec.MammogramCancerPrescribed))


                    If stSec.PAPSMEAR_Date IsNot Nothing Then

                        If stSec.PAPSMEAR_Date.Value IsNot Nothing AndAlso
                            stSec.PAPSMEAR_Date.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.PAPSMEAR_Date.Value.Value) Then

                            stSec.PAPSMEAR_Date.Value = Nothing

                        End If

                        .AddWithValue("PAPSMEAR_Date", VerifyDateNull(stSec.PAPSMEAR_Date))
                    End If

                    If stSec.PAPSMEAR_Result IsNot Nothing Then .AddWithValue("PAPSMEAR_Result", VerifyStringNull(stSec.PAPSMEAR_Result))
                    If stSec.PAPSMEAR_NAFor IsNot Nothing Then .AddWithValue("PAPSMEAR_NAFor", VerifyStringNull(stSec.PAPSMEAR_NAFor))
                    If stSec.PAPSMEAR_Prescribed IsNot Nothing Then .AddWithValue("PAPSMEAR_Prescribed", VerifyBooleanNull(stSec.PAPSMEAR_Prescribed))


                    If stSec.ProstateCancerDate IsNot Nothing Then

                        If stSec.ProstateCancerDate.Value IsNot Nothing AndAlso
                            stSec.ProstateCancerDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ProstateCancerDate.Value.Value) Then

                            stSec.ProstateCancerDate.Value = Nothing

                        End If

                        .AddWithValue("ProstateCancerDate", VerifyDateNull(stSec.ProstateCancerDate))
                    End If
                    If stSec.ProstateCancerResult IsNot Nothing Then .AddWithValue("ProstateCancerResult", VerifyStringNull(stSec.ProstateCancerResult))
                    If stSec.ProstateCancerNAFor IsNot Nothing Then .AddWithValue("ProstateCancerNAFor", VerifyStringNull(stSec.ProstateCancerNAFor))
                    If stSec.ProstateCancerPrescribed IsNot Nothing Then .AddWithValue("ProstateCancerPrescribed", VerifyBooleanNull(stSec.ProstateCancerPrescribed))

                    If stSec.Screening_HPV_Date IsNot Nothing Then
                        If stSec.Screening_HPV_Date.Value IsNot Nothing _
                            AndAlso stSec.Screening_HPV_Date.Value.HasValue _
                            AndAlso Not Globals.ValidateDateMinMaxRange(stSec.Screening_HPV_Date.Value.Value) Then

                            stSec.Screening_HPV_Date.Value = Nothing
                        End If

                        Try
                            If stSec.Screening_HPV_Date.Value = Date.MinValue Then
                                stSec.Screening_HPV_Date.Value = Nothing
                            End If
                        Catch ex As Exception
                        End Try

                        .AddWithValue("Screening_HPV_Date", VerifyDateNull(stSec.Screening_HPV_Date))
                    End If
                    If stSec.Screening_HPV_Comment IsNot Nothing Then .AddWithValue("Screening_HPV_Comment", VerifyStringNull(stSec.Screening_HPV_Comment))
                    If stSec.Screening_HPV_Result IsNot Nothing Then .AddWithValue("Screening_HPV_Result", VerifyStringNull(stSec.Screening_HPV_Result))
                    If stSec.Screening_HPV_Ordered IsNot Nothing Then .AddWithValue("Screening_HPV_Ordered", VerifyBooleanNull(stSec.Screening_HPV_Ordered))
                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveScreeningTest2023(ByVal claimKey As Long, ByVal stSec As ScreeningScheduleSection,
                                  ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If stSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveScreeningTest2023"
                'cmd.CommandText = "uspSaveScreeningTest"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)

                    If stSec.BoneMineralDensityDate IsNot Nothing Then

                        If stSec.BoneMineralDensityDate.Value IsNot Nothing AndAlso
                            stSec.BoneMineralDensityDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.BoneMineralDensityDate.Value.Value) Then

                            stSec.BoneMineralDensityDate.Value = Nothing

                        End If

                        .AddWithValue("BMDDate", VerifyDateNull(stSec.BoneMineralDensityDate))

                    End If
                    'AHA 2023 changes
                    .AddWithValue("Retinopathy", VerifyBooleanNull(stSec.Retinopathy))
                    .AddWithValue("Proliferative", VerifyBooleanNull(stSec.Proliferative))
                    .AddWithValue("ProliferativeEyeRT", VerifyBooleanNull(stSec.ProliferativeEyeRT))
                    .AddWithValue("ProliferativeEyeLT", VerifyBooleanNull(stSec.ProliferativeEyeLT))

                    'AHA 2020 Changes'
                    If stSec.BoneMineralDensityResult_Normal IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Normal", VerifyBooleanNull(stSec.BoneMineralDensityResult_Normal))
                    If stSec.BoneMineralDensityResult_Osteopenia IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Osteopenia", VerifyBooleanNull(stSec.BoneMineralDensityResult_Osteopenia))
                    If stSec.BoneMineralDensityResult_Osteoporosis IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Osteoporosis", VerifyBooleanNull(stSec.BoneMineralDensityResult_Osteoporosis))
                    If stSec.BoneMineralDensityResult_NA IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_NA", VerifyBooleanNull(stSec.BoneMineralDensityResult_NA))
                    If stSec.BoneMineralDensityResult_Other IsNot Nothing Then .AddWithValue("BoneMineralDensityResult_Other", VerifyBooleanNull(stSec.BoneMineralDensityResult_Other))



                    If stSec.BoneMineralDensityResult IsNot Nothing Then .AddWithValue("BMDResult", VerifyStringNull(stSec.BoneMineralDensityResult))
                    '.AddWithValue("BMDReviewed", VerifyBooleanNull(stSec.BoneMineralDensityReviewed))
                    If stSec.BoneMineralDensityNAFor IsNot Nothing Then .AddWithValue("BoneMineralDensity_NAFor", VerifyStringNull(stSec.BoneMineralDensityNAFor))
                    If stSec.BoneMineralDensityPrescribed IsNot Nothing Then .AddWithValue("BMDPrescribed", VerifyBooleanNull(stSec.BoneMineralDensityPrescribed))
                    If stSec.BoneMineralDensityRxOrdered IsNot Nothing Then .AddWithValue("BMDRxOrdered", VerifyBooleanNull(stSec.BoneMineralDensityRxOrdered))

                    If stSec.CardiovascularLDLDate IsNot Nothing Then

                        If stSec.CardiovascularLDLDate.Value IsNot Nothing AndAlso
                            stSec.CardiovascularLDLDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.CardiovascularLDLDate.Value.Value) Then

                            stSec.CardiovascularLDLDate.Value = Nothing

                        End If

                        .AddWithValue("CardiovascularLDLDate", VerifyDateNull(stSec.CardiovascularLDLDate))
                    End If
                    If stSec.CardiovascularLDLResult IsNot Nothing Then .AddWithValue("CardiovascularLDLResult", VerifyStringNull(stSec.CardiovascularLDLResult))
                    '.AddWithValue("CardiovascularLDLReviewed", VerifyBooleanNull(stSec.CardiovascularLDLReviewed))
                    If stSec.CardiovascularLDLNAFor IsNot Nothing Then .AddWithValue("Screening_Cardio_LDL_NAFor", VerifyStringNull(stSec.CardiovascularLDLNAFor))
                    If stSec.CardiovascularLDLPrescribed IsNot Nothing Then .AddWithValue("CardiovascularLDLPrescribed", VerifyBooleanNull(stSec.CardiovascularLDLPrescribed))

                    If stSec.CardiovascularBetaDate IsNot Nothing Then

                        If stSec.CardiovascularBetaDate.Value IsNot Nothing AndAlso
                            stSec.CardiovascularBetaDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.CardiovascularBetaDate.Value.Value) Then

                            stSec.CardiovascularBetaDate.Value = Nothing

                        End If

                        .AddWithValue("CardiovascularBetaDate", VerifyDateNull(stSec.CardiovascularBetaDate))
                    End If

                    If stSec.CardiovascularBetaResult IsNot Nothing Then .AddWithValue("CardiovascularBetaResult", VerifyStringNull(stSec.CardiovascularBetaResult))
                    '.AddWithValue("CardiovascularBetaReviewed", VerifyBooleanNull(stSec.CardiovascularBetaReviewed))
                    If stSec.CardiovascularBetaNAFor IsNot Nothing Then .AddWithValue("Screening_Cardio_BetaBlocke_NAFor", VerifyStringNull(stSec.CardiovascularBetaNAFor))
                    If stSec.CardiovascularBetaPrescribed IsNot Nothing Then .AddWithValue("CardiovascularBetaPrescribed", VerifyNull(stSec.CardiovascularBetaPrescribed.Value))

                    If stSec.ColorectalCancerScreeningSelectedIndex IsNot Nothing Then .AddWithValue("ColorectalCancerSelectedIndex", VerifyNull(stSec.ColorectalCancerScreeningSelectedIndex.Value))

                    If stSec.ColorectalCancerScreeningDate IsNot Nothing Then

                        If stSec.ColorectalCancerScreeningDate.Value IsNot Nothing AndAlso stSec.ColorectalCancerScreeningDate.Value.HasValue AndAlso Not Globals.ValidateDateMinMaxRange(stSec.ColorectalCancerScreeningDate.Value.Value) Then
                            stSec.ColorectalCancerScreeningDate.Value = Nothing
                        End If

                        .AddWithValue("ColorectalCancerDoneDate", VerifyDateNull(stSec.ColorectalCancerScreeningDate))
                    End If

                    If stSec.ColorectalCancerScreeningResult IsNot Nothing Then .AddWithValue("ColorectalCancerResult", VerifyStringNull(stSec.ColorectalCancerScreeningResult))
                    '.AddWithValue("ColorectalCancerReviewed", VerifyBooleanNull(stSec.ColorectalCancerScreeningReviewed))
                    If stSec.ColorectalCancerScreeningNAFor IsNot Nothing Then .AddWithValue("Screening_ColorectalCancer_NAFor", VerifyStringNull(stSec.ColorectalCancerScreeningNAFor))
                    If stSec.ColorectalCancerScreeningPrescribed IsNot Nothing Then .AddWithValue("ColorectalCancerPrescribed", VerifyBooleanNull(stSec.ColorectalCancerScreeningPrescribed))

                    If stSec.IsDiabetic IsNot Nothing Then .AddWithValue("IsDiabetic", VerifyBooleanNull(stSec.IsDiabetic))

                    If stSec.DiabetesScreening_DilatedEyeExamDate IsNot Nothing Then

                        If stSec.DiabetesScreening_DilatedEyeExamDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_DilatedEyeExamDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_DilatedEyeExamDate.Value.Value) Then

                            stSec.DiabetesScreening_DilatedEyeExamDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesDilatedEyeDate", VerifyDateNull(stSec.DiabetesScreening_DilatedEyeExamDate))
                    End If

                    If stSec.DiabetesScreening_DilatedEyeExamResult IsNot Nothing Then .AddWithValue("DiabetesDilatedEyeResult", VerifyStringNull(stSec.DiabetesScreening_DilatedEyeExamResult))
                    '.AddWithValue("DiabetesDilatedEyeReviewed", VerifyBooleanNull(stSec.DiabetesScreening_DilatedEyeExamReviewed))
                    If stSec.DiabetesScreening_DilatedEyeExamNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_DilatedEye_NAFor", VerifyStringNull(stSec.DiabetesScreening_DilatedEyeExamNAFor))
                    If stSec.DiabetesScreening_DilatedEyeExamPrescribed IsNot Nothing Then .AddWithValue("DiabetesDilatedEyePrescribed", VerifyBooleanNull(stSec.DiabetesScreening_DilatedEyeExamPrescribed))

                    If stSec.DiabetesScreening_LDLDate IsNot Nothing Then

                        If stSec.DiabetesScreening_LDLDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_LDLDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_LDLDate.Value.Value) Then

                            stSec.DiabetesScreening_LDLDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesLDLDate", VerifyDateNull(stSec.DiabetesScreening_LDLDate))
                    End If


                    If stSec.DiabetesScreening_LDLResult IsNot Nothing Then .AddWithValue("DiabetesLDLResult", VerifyStringNull(stSec.DiabetesScreening_LDLResult))
                    '.AddWithValue("DiabetesLDLReviewed", VerifyBooleanNull(stSec.DiabetesScreening_LDLReviewed))
                    If stSec.DiabetesScreening_LDLNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_LDL_NAFor", VerifyStringNull(stSec.DiabetesScreening_LDLNAFor))
                    If stSec.DiabetesScreening_LDLPrescribed IsNot Nothing Then .AddWithValue("DiabetesLDLPrescribed", VerifyBooleanNull(stSec.DiabetesScreening_LDLPrescribed))

                    If stSec.DiabetesScreening_HGA1C_Date IsNot Nothing Then

                        If stSec.DiabetesScreening_HGA1C_Date.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_HGA1C_Date.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_HGA1C_Date.Value.Value) Then

                            stSec.DiabetesScreening_HGA1C_Date.Value = Nothing

                        End If

                        .AddWithValue("DiabetesScreening_HGA1C_Date", VerifyDateNull(stSec.DiabetesScreening_HGA1C_Date))
                    End If


                    If stSec.DiabetesScreening_HGA1C_Result IsNot Nothing Then .AddWithValue("DiabetesScreening_HGA1C_Result", VerifyStringNull(stSec.DiabetesScreening_HGA1C_Result))
                    If stSec.DiabetesScreening_HGA1C_NAFor IsNot Nothing Then .AddWithValue("DiabetesScreening_HGA1C_NAFor", VerifyStringNull(stSec.DiabetesScreening_HGA1C_NAFor))
                    If stSec.DiabetesScreening_HGA1C_Prescribed IsNot Nothing Then .AddWithValue("DiabetesScreening_HGA1C_Prescribed", VerifyBooleanNull(stSec.DiabetesScreening_HGA1C_Prescribed))


                    'If stSec.DiabetesScreening_MicroalbuminDate IsNot Nothing Then

                    '    If stSec.DiabetesScreening_MicroalbuminDate.Value IsNot Nothing AndAlso
                    '        stSec.DiabetesScreening_MicroalbuminDate.Value.HasValue AndAlso
                    '        Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_MicroalbuminDate.Value.Value) Then

                    '        stSec.DiabetesScreening_MicroalbuminDate.Value = Nothing

                    '    End If
                    '    .AddWithValue("DiabetesMicroalbuminDate", VerifyDateNull(stSec.DiabetesScreening_MicroalbuminDate))
                    'End If


                    'If stSec.DiabetesScreening_MicroalbuminResult IsNot Nothing Then .AddWithValue("DiabetesMicroalbuminResult", VerifyStringNull(stSec.DiabetesScreening_MicroalbuminResult))
                    ''.AddWithValue("DiabetesMicroalbuminReviewed", VerifyBooleanNull(stSec.DiabetesScreening_MicroalbuminReviewed))
                    'If stSec.DiabetesScreening_MicroalbuminNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_Microalbumin_NAFor", VerifyStringNull(stSec.DiabetesScreening_MicroalbuminNAFor))
                    'If stSec.DiabetesScreening_MicroalbuminPrescribed IsNot Nothing Then .AddWithValue("DiabetesMicroalbuminPrescribed", VerifyBooleanNull(stSec.DiabetesScreening_MicroalbuminPrescribed))

                    'If stSec.GlaucomaTestDate IsNot Nothing Then

                    '    If stSec.GlaucomaTestDate.Value IsNot Nothing AndAlso
                    '        stSec.GlaucomaTestDate.Value.HasValue AndAlso
                    '        Not Globals.ValidateDateMinMaxRange(stSec.GlaucomaTestDate.Value.Value) Then

                    '        stSec.GlaucomaTestDate.Value = Nothing

                    '    End If

                    '    .AddWithValue("DiabetesGlaucomaTestDate", VerifyDateNull(stSec.GlaucomaTestDate))
                    'End If

                    'albumine
                    If stSec.DiabetesScreening_Urine_AlbuminDate IsNot Nothing Then

                        If stSec.DiabetesScreening_Urine_AlbuminDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_Urine_AlbuminDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_Urine_AlbuminDate.Value.Value) Then

                            stSec.DiabetesScreening_Urine_AlbuminDate.Value = Nothing

                        End If

                        .AddWithValue("Screening_Diabetes_Urine_Albumin_Date", VerifyDateNull(stSec.DiabetesScreening_Urine_AlbuminDate))
                    End If
                    If stSec.DiabetesScreening_Urine_AlbuminResult IsNot Nothing Then .AddWithValue("Screening_Diabetes_Urine_Albumin_Result", VerifyDecimalNull(stSec.DiabetesScreening_Urine_AlbuminResult))
                    If stSec.DiabetesScreening_Urine_AlbuminNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_Urine_Albumin_Comment", VerifyStringNull(stSec.DiabetesScreening_Urine_AlbuminNAFor))
                    If stSec.DiabetesScreening_Urine_AlbuminPrescribed IsNot Nothing Then .AddWithValue("Screening_Diabetes_Urine_Albumin_Prescribed", VerifyBooleanNull(stSec.DiabetesScreening_Urine_AlbuminPrescribed))

                    'creatinine
                    If stSec.DiabetesScreening_Urine_CreatinineDate IsNot Nothing Then

                        If stSec.DiabetesScreening_Urine_CreatinineDate.Value IsNot Nothing AndAlso
                            stSec.DiabetesScreening_Urine_CreatinineDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.DiabetesScreening_Urine_CreatinineDate.Value.Value) Then

                            stSec.DiabetesScreening_Urine_CreatinineDate.Value = Nothing

                        End If

                        .AddWithValue("Screening_Diabetes_Urine_Creatinine_Date", VerifyDateNull(stSec.DiabetesScreening_Urine_CreatinineDate))
                    End If
                    If stSec.DiabetesScreening_Urine_CreatinineResult IsNot Nothing Then .AddWithValue("Screening_Diabetes_Urine_Creatinine_Result", VerifyDecimalNull(stSec.DiabetesScreening_Urine_CreatinineResult))
                    If stSec.DiabetesScreening_Urine_CreatinineNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_Urine_Creatinine_Comment", VerifyStringNull(stSec.DiabetesScreening_Urine_CreatinineNAFor))
                    If stSec.DiabetesScreening_Urine_CreatininePrescribed IsNot Nothing Then .AddWithValue("Screening_Diabetes_Urine_Creatinine_Prescribed", VerifyBooleanNull(stSec.DiabetesScreening_Urine_CreatininePrescribed))

                    'ratio
                    If stSec.DiabetesScreening_CreatinineAlbumine_Ratio IsNot Nothing Then .AddWithValue("Screening_Diabetes_Albumin_Creatinine_Ratio", VerifyDecimalNull(stSec.DiabetesScreening_CreatinineAlbumine_Ratio))
                    '

                    If stSec.GlaucomaTestResult IsNot Nothing Then .AddWithValue("DiabetesGlaucomaResult", VerifyStringNull(stSec.GlaucomaTestResult))
                    '.AddWithValue("DiabetesGlaucomaReviewed", VerifyBooleanNull(stSec.GlaucomaTestReviewed))
                    If stSec.GlaucomaTestNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_GlaucomaTest_NAFor", VerifyStringNull(stSec.GlaucomaTestNAFor))
                    If stSec.GlaucomaTestPrescribed IsNot Nothing Then .AddWithValue("DiabetesGlaucomaPrescribed", VerifyBooleanNull(stSec.GlaucomaTestPrescribed))

                    If stSec.Mammogram_ProstateCancerDate IsNot Nothing Then

                        If stSec.Mammogram_ProstateCancerDate.Value IsNot Nothing AndAlso
                            stSec.Mammogram_ProstateCancerDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.Mammogram_ProstateCancerDate.Value.Value) Then

                            stSec.Mammogram_ProstateCancerDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesMammogramProstateDate", VerifyDateNull(stSec.Mammogram_ProstateCancerDate))
                    End If

                    If stSec.Mammogram_ProstateCancerResult IsNot Nothing Then .AddWithValue("DiabetesMammogramProstateResult", VerifyStringNull(stSec.Mammogram_ProstateCancerResult))
                    '.AddWithValue("DiabetesMammogramProstateReviewed", VerifyBooleanNull(stSec.Mammogram_ProstateCancerReviewed))
                    If stSec.Mammogram_ProstateCancerNAFor IsNot Nothing Then .AddWithValue("Screening_Diabetes_MammogramProstate_NAFor", VerifyStringNull(stSec.Mammogram_ProstateCancerNAFor))
                    If stSec.Mammogram_ProstateCancerPrescribed IsNot Nothing Then .AddWithValue("DiabetesMammogramProstatePrescribe", VerifyBooleanNull(stSec.Mammogram_ProstateCancerPrescribed))

                    If stSec.FluShotDate IsNot Nothing Then

                        If stSec.FluShotDate.Value IsNot Nothing AndAlso
                            stSec.FluShotDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.FluShotDate.Value.Value) Then

                            stSec.FluShotDate.Value = Nothing

                        End If

                        .AddWithValue("DiabetesFluShotDate", VerifyDateNull(stSec.FluShotDate))
                    End If


                    If stSec.FluShotComments IsNot Nothing Then .AddWithValue("DiabetesFluShotComments", VerifyStringNull(stSec.FluShotComments))
                    If stSec.FluShotPrescribed IsNot Nothing Then .AddWithValue("DiabetesFluShotPrescribed", VerifyBooleanNull(stSec.FluShotPrescribed))
                    'AHA 2020 Changes'
                    If stSec.FluShot_PatientRefuses IsNot Nothing Then .AddWithValue("DiabetesFluShotPatientRefuses", VerifyBooleanNull(stSec.FluShot_PatientRefuses))

                    If stSec.PneumococcalShotDate IsNot Nothing Then

                        If stSec.PneumococcalShotDate.Value IsNot Nothing AndAlso
                            stSec.PneumococcalShotDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.PneumococcalShotDate.Value.Value) Then

                            stSec.PneumococcalShotDate.Value = Nothing

                        End If
                        .AddWithValue("DiabetesPneumococcalShotDate", VerifyDateNull(stSec.PneumococcalShotDate))
                    End If

                    If stSec.PneumococcalShotComments IsNot Nothing Then .AddWithValue("DiabetesPneumococcalComments", VerifyStringNull(stSec.PneumococcalShotComments))
                    If stSec.PneumococcalShotPrescribed IsNot Nothing Then .AddWithValue("DiabetesPneumococcalPrescribed", VerifyBooleanNull(stSec.PneumococcalShotPrescribed))
                    'AHA 2020 Changes'
                    If stSec.PneumococcalShot_PatientRefuses IsNot Nothing Then .AddWithValue("DiabetesPneumococcalPatientRefuses", VerifyBooleanNull(stSec.PneumococcalShot_PatientRefuses))

                    'AHA 2023 Changes'
                    If stSec.TdTdap_Comments IsNot Nothing Then .AddWithValue("Screening_Vaccine_TdTdap_Comments", VerifyStringNull(stSec.TdTdap_Comments))
                    If stSec.TdTdap_Prescribed IsNot Nothing Then .AddWithValue("Screening_Vaccine_TdTdap_Prescribed", VerifyBooleanNull(stSec.TdTdap_Prescribed))
                    If stSec.TdTdap_PatientRefuses IsNot Nothing Then .AddWithValue("Screening_Vaccine_TdTdap_PatientRefuses", VerifyBooleanNull(stSec.TdTdap_PatientRefuses))

                    If stSec.TdTdap_Done_Date IsNot Nothing Then
                        If stSec.TdTdap_Done_Date.Value IsNot Nothing AndAlso
                            stSec.TdTdap_Done_Date.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.TdTdap_Done_Date.Value.Value) Then
                            stSec.TdTdap_Done_Date.Value = Nothing
                        End If
                        .AddWithValue("Screening_Vaccine_TdTdap_Done_Date", VerifyDateNull(stSec.TdTdap_Done_Date))
                    End If

                    If stSec.ZosterVaccineOrdered IsNot Nothing Then .AddWithValue("@Screening_Vaccine_ZosterVaccineOrdered", VerifyBooleanNull(stSec.ZosterVaccineOrdered))
                    If stSec.ZosterVaccineRefuse IsNot Nothing Then .AddWithValue("Screening_Vaccine_ZosterVaccineRefuse", VerifyBooleanNull(stSec.ZosterVaccineRefuse))
                    If stSec.ZosterVaccineShotDate1 IsNot Nothing AndAlso stSec.ZosterVaccineShotDate1.Value IsNot Nothing AndAlso stSec.ZosterVaccineShotDate1.Value.HasValue Then
                        .AddWithValue("@Screening_Vaccine_ZosterVaccineShotDate1", stSec.ZosterVaccineShotDate1.Value)
                    End If

                    If stSec.ZosterVaccineShotDate2 IsNot Nothing AndAlso stSec.ZosterVaccineShotDate2.Value IsNot Nothing AndAlso stSec.ZosterVaccineShotDate2.Value.HasValue Then
                        .AddWithValue("@Screening_Vaccine_ZosterVaccineShotDate2", stSec.ZosterVaccineShotDate2.Value)
                    End If



                    If stSec.ColorectalColonoscopy IsNot Nothing Then .AddWithValue("ColorectalColonoscopy", VerifyBooleanNull(stSec.ColorectalColonoscopy))

                    If stSec.ColorectalColonoscopyDate IsNot Nothing Then

                        If stSec.ColorectalColonoscopyDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalColonoscopyDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalColonoscopyDate.Value.Value) Then

                            stSec.ColorectalColonoscopyDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalColonoscopyDate", VerifyDateNull(stSec.ColorectalColonoscopyDate))
                    End If

                    'TODO: Se maneja los datos de Colorectal
                    'AHA 2020 Changes'
                    If stSec.Colorectal_ColonoscopyResult_NA IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_NA", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_NA))
                    If stSec.Colorectal_ColonoscopyResult_Negative IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Negative", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Negative))
                    If stSec.Colorectal_ColonoscopyResult_Diverticles IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Diverticles", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Diverticles))
                    If stSec.Colorectal_ColonoscopyResult_BleedingAreas IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_BleedingAreas", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_BleedingAreas))
                    If stSec.Colorectal_ColonoscopyResult_CAInColon IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_CAInColon", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_CAInColon))
                    If stSec.Colorectal_ColonoscopyResult_CAInRectum IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_CAInRectum", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_CAInRectum))
                    If stSec.Colorectal_ColonoscopyResult_Colitis IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Colitis", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Colitis))
                    If stSec.Colorectal_ColonoscopyResult_UlcerativeOlitis IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_UlcerativeOlitis", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_UlcerativeOlitis))
                    If stSec.Colorectal_ColonoscopyResult_CrohnsDisease IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_CrohnsDisease", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_CrohnsDisease))
                    If stSec.Colorectal_ColonoscopyResult_Polyps IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Polyps", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Polyps))
                    If stSec.Colorectal_ColonoscopyResult_Other IsNot Nothing Then .AddWithValue("Colorectal_ColonoscopyResult_Other", VerifyBooleanNull(stSec.Colorectal_ColonoscopyResult_Other))

                    'TODO: Se añade el valor de los resultados al campo para reporte.
                    'AHA 2020 Changes'
                    Dim Result As StringField = New StringField
                    Result.Value = ""

                    If stSec.ColorectalColonoscopyResult IsNot Nothing Then
                        stSec.ColorectalColonoscopyResult = New StringField()

                        If stSec.Colorectal_ColonoscopyResult_Negative.Value Then
                            Result.Value += "Negativo "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Diverticles.Value Then
                            Result.Value += "Diverticulos "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_BleedingAreas.Value Then
                            Result.Value += "Areas de Sangrado "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_CAInColon.Value Then
                            Result.Value += "CA en Colon "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_CAInRectum.Value Then
                            Result.Value += "CA en Recto "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Colitis.Value Then
                            Result.Value += "Colitis "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_UlcerativeOlitis.Value Then
                            Result.Value += "Ulceras "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_CrohnsDisease.Value Then
                            Result.Value += "Crohns "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Polyps.Value Then
                            Result.Value += "Polipos "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If
                        If stSec.Colorectal_ColonoscopyResult_Other.Value Then
                            Result.Value += "Otros "
                            '.AddWithValue("ColorectalColonoscopyResult", VerifyStringNull(stSec.ColorectalColonoscopyResult))
                        End If

                        stSec.ColorectalColonoscopyResult.Value = Result.Value
                    End If

                    If stSec.ColorectalColonoscopyNAFor IsNot Nothing Then .AddWithValue("ColorectalColonoscopyNAFor", VerifyStringNull(stSec.ColorectalColonoscopyNAFor))
                    If stSec.ColorectalColonoscopyPrescribe IsNot Nothing Then .AddWithValue("ColorectalColonoscopyPrescribe", VerifyBooleanNull(stSec.ColorectalColonoscopyPrescribe))

                    If stSec.ColorectalOccultBlood IsNot Nothing Then .AddWithValue("ColorectalOccultBlood", VerifyBooleanNull(stSec.ColorectalOccultBlood))

                    If stSec.ColorectalOccultBloodDate IsNot Nothing Then

                        If stSec.ColorectalOccultBloodDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalOccultBloodDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalOccultBloodDate.Value.Value) Then

                            stSec.ColorectalOccultBloodDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalOccultBloodDate", VerifyDateNull(stSec.ColorectalOccultBloodDate))
                    End If

                    If stSec.ColorectalOccultBloodResult IsNot Nothing Then .AddWithValue("ColorectalOccultBloodResult", VerifyStringNull(stSec.ColorectalOccultBloodResult))
                    If stSec.ColorectalOccultBloodNAFor IsNot Nothing Then .AddWithValue("ColorectalOccultBloodNAFor", VerifyStringNull(stSec.ColorectalOccultBloodNAFor))
                    If stSec.ColorectalOccultBloodPrescribe IsNot Nothing Then .AddWithValue("ColorectalOccultBloodPrescribe", VerifyBooleanNull(stSec.ColorectalOccultBloodPrescribe))

                    If stSec.ColorectalFlexibleSigmoidoscopy IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopy", VerifyBooleanNull(stSec.ColorectalFlexibleSigmoidoscopy))

                    If stSec.ColorectalFlexibleSigmoidoscopyDate IsNot Nothing Then

                        If stSec.ColorectalFlexibleSigmoidoscopyDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalFlexibleSigmoidoscopyDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalFlexibleSigmoidoscopyDate.Value.Value) Then

                            stSec.ColorectalFlexibleSigmoidoscopyDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalFlexibleSigmoidoscopyDate", VerifyDateNull(stSec.ColorectalFlexibleSigmoidoscopyDate))
                    End If

                    'AHA 2020 Changes'
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_NA IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_NA", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_NA))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_Negative IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_Negative", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_Negative))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_AnalFissure IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_AnalFissure", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_AnalFissure))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_AnorectalAbscess))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_IntestinalOcclusion))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_CAEnElSigmoideo))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInRectum IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_CAInRectum", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInRectum))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_CAInTheRectosigmoidJunction))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_ColorectalPolyps))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_Diverticles IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_Diverticles", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_Diverticles))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_Hemorrhoids))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_HirschsprungDisease))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammatoryBowelDisease))
                    If stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection IsNot Nothing Then .AddWithValue("Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection", VerifyBooleanNull(stSec.Colorectal_FlexibleSigmoidoscopyResult_InflammationOrInfection))


                    If stSec.ColorectalFlexibleSigmoidoscopyResult IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopyResult", VerifyStringNull(stSec.ColorectalFlexibleSigmoidoscopyResult))
                    If stSec.ColorectalFlexibleSigmoidoscopyNAFor IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopyNAFor", VerifyStringNull(stSec.ColorectalFlexibleSigmoidoscopyNAFor))
                    If stSec.ColorectalFlexibleSigmoidoscopyPrescribe IsNot Nothing Then .AddWithValue("ColorectalFlexibleSigmoidoscopyPrescribe", VerifyBooleanNull(stSec.ColorectalFlexibleSigmoidoscopyPrescribe))

                    If stSec.ColorectalFITDNA IsNot Nothing Then .AddWithValue("ColorectalFITDNA", VerifyBooleanNull(stSec.ColorectalFITDNA))

                    If stSec.ColorectalFITDNADate IsNot Nothing Then

                        If stSec.ColorectalFITDNADate.Value IsNot Nothing AndAlso
                            stSec.ColorectalFITDNADate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalFITDNADate.Value.Value) Then

                            stSec.ColorectalFITDNADate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalFITDNADate", VerifyDateNull(stSec.ColorectalFITDNADate))
                    End If

                    If stSec.ColorectalFITDNAResult IsNot Nothing Then .AddWithValue("ColorectalFITDNAResult", VerifyStringNull(stSec.ColorectalFITDNAResult))
                    If stSec.ColorectalFITDNANAFor IsNot Nothing Then .AddWithValue("ColorectalFITDNANAFor", VerifyStringNull(stSec.ColorectalFITDNANAFor))
                    If stSec.ColorectalFITDNAPrescribe IsNot Nothing Then .AddWithValue("ColorectalFITDNAPrescribe", VerifyBooleanNull(stSec.ColorectalFITDNAPrescribe))
                    If stSec.ColorectalColonographyCT IsNot Nothing Then .AddWithValue("ColorectalColonographyCT", VerifyBooleanNull(stSec.ColorectalColonographyCT))
                    If stSec.ColorectalColonographyCTDate IsNot Nothing Then

                        If stSec.ColorectalColonographyCTDate.Value IsNot Nothing AndAlso
                            stSec.ColorectalColonographyCTDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ColorectalColonographyCTDate.Value.Value) Then

                            stSec.ColorectalColonographyCTDate.Value = Nothing

                        End If

                        .AddWithValue("ColorectalColonographyCTDate", VerifyDateNull(stSec.ColorectalColonographyCTDate))
                    End If

                    If stSec.ColorectalColonographyCTResult IsNot Nothing Then .AddWithValue("ColorectalColonographyCTResult", VerifyStringNull(stSec.ColorectalColonographyCTResult))
                    If stSec.ColorectalColonographyCTNAFor IsNot Nothing Then .AddWithValue("ColorectalColonographyCTNAFor", VerifyStringNull(stSec.ColorectalColonographyCTNAFor))
                    If stSec.ColorectalColonographyCTPrescribe IsNot Nothing Then .AddWithValue("ColorectalColonographyCTPrescribe", VerifyBooleanNull(stSec.ColorectalColonographyCTPrescribe))

                    If stSec.MammogramCancerDate IsNot Nothing Then

                        If stSec.MammogramCancerDate.Value IsNot Nothing AndAlso
                            stSec.MammogramCancerDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.MammogramCancerDate.Value.Value) Then

                            stSec.MammogramCancerDate.Value = Nothing

                        End If

                        .AddWithValue("MammogramCancerDate", VerifyDateNull(stSec.MammogramCancerDate))
                    End If

                    'AHA 2020 Changes'
                    If stSec.Mammogram_CancerResult_Category_0 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_0", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_0))
                    If stSec.Mammogram_CancerResult_Category_1 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_1", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_1))
                    If stSec.Mammogram_CancerResult_Category_2 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_2", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_2))
                    If stSec.Mammogram_CancerResult_Category_3 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_3", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_3))
                    If stSec.Mammogram_CancerResult_Category_4 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_4", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_4))
                    If stSec.Mammogram_CancerResult_Category_5 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_5", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_5))
                    If stSec.Mammogram_CancerResult_Category_6 IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_Category_6", VerifyBooleanNull(stSec.Mammogram_CancerResult_Category_6))
                    If stSec.Mammogram_CancerResult_NA IsNot Nothing Then .AddWithValue("Mammogram_CancerResult_NA", VerifyBooleanNull(stSec.Mammogram_CancerResult_NA))

                    If stSec.MammogramCancerResult IsNot Nothing Then .AddWithValue("MammogramCancerResult", VerifyStringNull(stSec.MammogramCancerResult))
                    If stSec.MammogramCancerNAFor IsNot Nothing Then .AddWithValue("MammogramCancerNAFor", VerifyStringNull(stSec.MammogramCancerNAFor))
                    If stSec.MammogramCancerPrescribed IsNot Nothing Then .AddWithValue("MammogramCancerPrescribed", VerifyBooleanNull(stSec.MammogramCancerPrescribed))


                    If stSec.PAPSMEAR_Date IsNot Nothing Then

                        If stSec.PAPSMEAR_Date.Value IsNot Nothing AndAlso
                            stSec.PAPSMEAR_Date.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.PAPSMEAR_Date.Value.Value) Then

                            stSec.PAPSMEAR_Date.Value = Nothing

                        End If

                        .AddWithValue("PAPSMEAR_Date", VerifyDateNull(stSec.PAPSMEAR_Date))
                    End If

                    If stSec.PAPSMEAR_Result IsNot Nothing Then .AddWithValue("PAPSMEAR_Result", VerifyStringNull(stSec.PAPSMEAR_Result))
                    If stSec.PAPSMEAR_NAFor IsNot Nothing Then .AddWithValue("PAPSMEAR_NAFor", VerifyStringNull(stSec.PAPSMEAR_NAFor))
                    If stSec.PAPSMEAR_Prescribed IsNot Nothing Then .AddWithValue("PAPSMEAR_Prescribed", VerifyBooleanNull(stSec.PAPSMEAR_Prescribed))


                    If stSec.ProstateCancerDate IsNot Nothing Then

                        If stSec.ProstateCancerDate.Value IsNot Nothing AndAlso
                            stSec.ProstateCancerDate.Value.HasValue AndAlso
                            Not Globals.ValidateDateMinMaxRange(stSec.ProstateCancerDate.Value.Value) Then

                            stSec.ProstateCancerDate.Value = Nothing

                        End If

                        .AddWithValue("ProstateCancerDate", VerifyDateNull(stSec.ProstateCancerDate))
                    End If
                    If stSec.ProstateCancerResult IsNot Nothing Then .AddWithValue("ProstateCancerResult", VerifyStringNull(stSec.ProstateCancerResult))
                    If stSec.ProstateCancerNAFor IsNot Nothing Then .AddWithValue("ProstateCancerNAFor", VerifyStringNull(stSec.ProstateCancerNAFor))
                    If stSec.ProstateCancerPrescribed IsNot Nothing Then .AddWithValue("ProstateCancerPrescribed", VerifyBooleanNull(stSec.ProstateCancerPrescribed))

                    If stSec.Screening_HPV_Date IsNot Nothing Then
                        If stSec.Screening_HPV_Date.Value IsNot Nothing _
                            AndAlso stSec.Screening_HPV_Date.Value.HasValue _
                            AndAlso Not Globals.ValidateDateMinMaxRange(stSec.Screening_HPV_Date.Value.Value) Then

                            stSec.Screening_HPV_Date.Value = Nothing
                        End If

                        Try
                            If stSec.Screening_HPV_Date.Value = Date.MinValue Then
                                stSec.Screening_HPV_Date.Value = Nothing
                            End If
                        Catch ex As Exception
                        End Try

                        .AddWithValue("Screening_HPV_Date", VerifyDateNull(stSec.Screening_HPV_Date))
                    End If
                    If stSec.Screening_HPV_Comment IsNot Nothing Then .AddWithValue("Screening_HPV_Comment", VerifyStringNull(stSec.Screening_HPV_Comment))
                    If stSec.Screening_HPV_Result IsNot Nothing Then .AddWithValue("Screening_HPV_Result", VerifyStringNull(stSec.Screening_HPV_Result))
                    If stSec.Screening_HPV_Ordered IsNot Nothing Then .AddWithValue("Screening_HPV_Ordered", VerifyBooleanNull(stSec.Screening_HPV_Ordered))
                    If stSec.Retinopathy_Negative IsNot Nothing Then .AddWithValue("Retinopathy_Negative", VerifyBooleanNull(stSec.Retinopathy_Negative))
                    If stSec.Retinopathy_Negative_Eye IsNot Nothing Then .AddWithValue("Retinopathy_Negative_Eye", VerifyIntegerNull(stSec.Retinopathy_Negative_Eye))

                    If stSec.Screening_Eye_Severity IsNot Nothing Then .AddWithValue("Screening_Eye_Severity", VerifyBooleanNull(stSec.Screening_Eye_Severity))
                    If stSec.Screening_Eye_SeverityLevel IsNot Nothing Then .AddWithValue("Screening_Eye_SeverityLevel", VerifyIntegerNull(stSec.Screening_Eye_SeverityLevel))

                    If stSec.Screening_Proliferative_Eye IsNot Nothing Then .AddWithValue("Screening_Proliferative_Eye", VerifyIntegerNull(stSec.Screening_Proliferative_Eye))
                    If stSec.Screening_Retinopathy_Eye IsNot Nothing Then .AddWithValue("Screening_Retinopathy_Eye", VerifyIntegerNull(stSec.Screening_Retinopathy_Eye))

                    If stSec.MacularEdema IsNot Nothing Then .AddWithValue("Screening_MacularEdema", VerifyBooleanNull(stSec.MacularEdema))
                    If stSec.MacularEdema_Eye IsNot Nothing Then .AddWithValue("Screening_MacularEdema_Eye", VerifyIntegerNull(stSec.MacularEdema_Eye))
                    If stSec.ScreeningRetinopathy_NA IsNot Nothing Then .AddWithValue("ScreeningRetinopathy_NA", VerifyBooleanNull(stSec.ScreeningRetinopathy_NA))
                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub


    Private Sub SavePhysicalExamination(ByVal claimKey As Long, ByVal peSec As PhysicalExaminationSection,
                                        ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If peSec IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSavePhysicalExamination"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)

                    If peSec.Temperature Is Nothing Then
                        peSec.Temperature = New DecimalField
                        peSec.Temperature.Value = 0.0
                    End If

                    .AddWithValue("Temperature", peSec.Temperature.Value)
                    .AddWithValue("TemperatureType", VerifyStringNull(peSec.TemperatureType))

                    If peSec.Pulse IsNot Nothing Then .AddWithValue("Pulse", peSec.Pulse.Value)

                    If peSec.Breathing IsNot Nothing Then .AddWithValue("Breathing", peSec.Breathing.Value)
                    If peSec.BloodPresure1 IsNot Nothing Then .AddWithValue("BloodPresure1", peSec.BloodPresure1.Value)
                    If peSec.BloodPresure2 IsNot Nothing Then .AddWithValue("BloodPresure2", peSec.BloodPresure2.Value)

                    If peSec.Height Is Nothing Then
                        peSec.Height = New DecimalField
                        peSec.Height.Value = 0.0
                    End If

                    .AddWithValue("Height", peSec.Height.Value)
                    .AddWithValue("HeightType", VerifyStringNull(peSec.HeightType))



                    If peSec.Weight Is Nothing Then
                        peSec.Weight = New DecimalField
                        peSec.Weight.Value = 0.0
                    End If

                    If peSec.WeightType Is Nothing Then
                        peSec.WeightType = New StringField
                        peSec.WeightType.Value = "lbs"
                    End If

                    .AddWithValue("Weight", peSec.Weight.Value)
                    .AddWithValue("WeightType", VerifyStringNull(peSec.WeightType))

                    If peSec.BMI Is Nothing Then
                        peSec.BMI = New DecimalField
                        peSec.BMI.Value = 0.0
                    End If

                    .AddWithValue("BMI", peSec.BMI.Value)

                    .AddWithValue("HEENTNotes", VerifyStringNull(peSec.HEENOralNotes))

                    If peSec.HEENOralOptions IsNot Nothing Then
                        .AddWithValue("HEENOralOptions_Peerl", VerifyBooleanNull(peSec.HEENOralOptions.PEERL))
                        .AddWithValue("HEENOralOptions_NoTeeth", VerifyBooleanNull(peSec.HEENOralOptions.NoTeeth))
                        .AddWithValue("HEENOralOptions_DryMouth", VerifyBooleanNull(peSec.HEENOralOptions.DryMouth))
                        .AddWithValue("HEENOralOptions_DryNose", VerifyBooleanNull(peSec.HEENOralOptions.DryNose))
                        .AddWithValue("HEENOralOptions_BleedingGums", VerifyBooleanNull(peSec.HEENOralOptions.BleedingGums))
                        .AddWithValue("HEENOralOptions_WNL", VerifyBooleanNull(peSec.HEENOralOptions.WNL))

                        .AddWithValue("HEENOralOptions_Strabismus", VerifyBooleanNull(peSec.HEENOralOptions.Strabismus))
                        .AddWithValue("HEENOralOptions_Ptosis", VerifyBooleanNull(peSec.HEENOralOptions.Ptosis))
                        .AddWithValue("HEENOralOptions_Redreflex", VerifyBooleanNull(peSec.HEENOralOptions.RedReflex))
                        .AddWithValue("HEENOralOptions_AbnormalPupillaryReflex", VerifyBooleanNull(peSec.HEENOralOptions.AbnormalPupillaryReflex))
                        .AddWithValue("HEENOralOptions_BlockedNasolacrimalDucts", VerifyBooleanNull(peSec.HEENOralOptions.BlockedNasolacrimalDucts))
                        .AddWithValue("HEENOralOptions_NasalDischarge", VerifyBooleanNull(peSec.HEENOralOptions.NasalDischarge))
                        .AddWithValue("HEENOralOptions_ExudatingTonsils", VerifyBooleanNull(peSec.HEENOralOptions.ExudatingTonsils))

                        '2025
                        .AddWithValue("HEENOptions_Normocephalic", VerifyBooleanNull(peSec.HEENOralOptions.Normocephalic))
                        .AddWithValue("HEENOptions_ScalpLessionsMasses", VerifyBooleanNull(peSec.HEENOralOptions.ScalpLessionsMasses))
                        .AddWithValue("HEENOptions_NeckSupple", VerifyBooleanNull(peSec.HEENOralOptions.NeckSupple))
                        .AddWithValue("HEENOptions_Adenopathies", VerifyBooleanNull(peSec.HEENOralOptions.Adenopathies))
                        .AddWithValue("HEENOptions_ClearOropharynxn", VerifyBooleanNull(peSec.HEENOralOptions.ClearOropharynxn))
                        .AddWithValue("HEENOptions_LessionExudate", VerifyBooleanNull(peSec.HEENOralOptions.LessionExudate))
                        .AddWithValue("HEENOptions_TympanicMembranesIntact", VerifyBooleanNull(peSec.HEENOralOptions.TympanicMembranesIntact))
                        .AddWithValue("HEENOptions_EqualAirConductionAndAcousticReflexes", VerifyBooleanNull(peSec.HEENOralOptions.EqualAirConductionAndAcousticReflexes))
                        .AddWithValue("HEENOptions_NoNystagmus", VerifyBooleanNull(peSec.HEENOralOptions.NoNystagmus))
                        .AddWithValue("HEENOptions_EOMI", VerifyBooleanNull(peSec.HEENOralOptions.EOMI))
                        .AddWithValue("HEENOptions_Other", VerifyBooleanNull(peSec.HEENOralOptions.Other))
                        .AddWithValue("HEENOptions_None", VerifyBooleanNull(peSec.HEENOralOptions.None))

                    End If


                    '2025
                    .AddWithValue("ConstitutionalOptions_Notes", VerifyStringNull(peSec.ConstitutionalNotes))

                    If peSec.ConstitutionalOptions IsNot Nothing Then
                        .AddWithValue("ConstitutionalOptions_WellDeveloped", VerifyBooleanNull(peSec.ConstitutionalOptions.WellDeveloped))
                        .AddWithValue("ConstitutionalOptions_PoorDeveloped", VerifyBooleanNull(peSec.ConstitutionalOptions.PoorDeveloped))
                        .AddWithValue("ConstitutionalOptions_AdequateNourishment", VerifyBooleanNull(peSec.ConstitutionalOptions.AdequateNourishment))
                        .AddWithValue("ConstitutionalOptions_InadequateNourishment", VerifyBooleanNull(peSec.ConstitutionalOptions.InadequateNourishment))
                        .AddWithValue("ConstitutionalOptions_InAcuteDistress", VerifyBooleanNull(peSec.ConstitutionalOptions.InAcuteDistress))
                        .AddWithValue("ConstitutionalOptions_NoAcuteDistress", VerifyBooleanNull(peSec.ConstitutionalOptions.NoAcuteDistress))
                        .AddWithValue("ConstitutionalOptions_CAOX", VerifyBooleanNull(peSec.ConstitutionalOptions.CAOX))
                        .AddWithValue("ConstitutionalOptions_Others", VerifyBooleanNull(peSec.ConstitutionalOptions.Others))
                        .AddWithValue("ConstitutionalOptions_None", VerifyBooleanNull(peSec.ConstitutionalOptions.None))
                    End If

                    '2025
                    .AddWithValue("IntegumentaryOptions_Notes", VerifyStringNull(peSec.IntegumentaryNotes))

                    If peSec.IntegumentaryOptions IsNot Nothing Then
                        .AddWithValue("IntegumentaryOptions_Warm", VerifyBooleanNull(peSec.IntegumentaryOptions.Warm))
                        .AddWithValue("IntegumentaryOptions_Cold", VerifyBooleanNull(peSec.IntegumentaryOptions.Cold))
                        .AddWithValue("IntegumentaryOptions_AdequatePerfusion", VerifyBooleanNull(peSec.IntegumentaryOptions.AdequatePerfusion))
                        .AddWithValue("IntegumentaryOptions_InadequatePerfusion", VerifyBooleanNull(peSec.IntegumentaryOptions.InadequatePerfusion))
                        .AddWithValue("IntegumentaryOptions_AdequateSkinTurgor", VerifyBooleanNull(peSec.IntegumentaryOptions.AdequateSkinTurgor))
                        .AddWithValue("IntegumentaryOptions_InadequateSkinTurgor", VerifyBooleanNull(peSec.IntegumentaryOptions.InadequateSkinTurgor))
                        .AddWithValue("IntegumentaryOptions_Acne", VerifyBooleanNull(peSec.IntegumentaryOptions.Acne))
                        .AddWithValue("IntegumentaryOptions_Rash", VerifyBooleanNull(peSec.IntegumentaryOptions.Rash))
                        .AddWithValue("IntegumentaryOptions_SkinSpots", VerifyBooleanNull(peSec.IntegumentaryOptions.SkinSpots))
                        .AddWithValue("IntegumentaryOptions_Others", VerifyBooleanNull(peSec.IntegumentaryOptions.Others))
                        .AddWithValue("Integumentary_None", VerifyBooleanNull(peSec.IntegumentaryOptions.None))
                    End If

                    '2025
                    .AddWithValue("RespiratoryOptions_Notes", VerifyStringNull(peSec.RespiratoryNotes))

                    If peSec.RespiratoryOptions IsNot Nothing Then
                        .AddWithValue("RespiratoryOptions_ClearToAuscultations", VerifyBooleanNull(peSec.RespiratoryOptions.ClearToAuscultations))
                        .AddWithValue("RespiratoryOptions_Wheezes", VerifyBooleanNull(peSec.RespiratoryOptions.Wheezes))
                        .AddWithValue("RespiratoryOptions_RonchiOrRales", VerifyBooleanNull(peSec.RespiratoryOptions.RonchiOrRales))
                        .AddWithValue("RespiratoryOptions_AdequatePercussionSounds", VerifyBooleanNull(peSec.RespiratoryOptions.AdequatePercussionSounds))
                        .AddWithValue("RespiratoryOptions_InadequatePercussionSounds", VerifyBooleanNull(peSec.RespiratoryOptions.InadequatePercussionSounds))
                        .AddWithValue("RespiratoryOptions_PainUponPalpitation", VerifyBooleanNull(peSec.RespiratoryOptions.PainUponPalpitation))
                        .AddWithValue("RespiratoryOptions_Others", VerifyBooleanNull(peSec.RespiratoryOptions.Others))
                        .AddWithValue("RespiratoryOptions_None", VerifyBooleanNull(peSec.RespiratoryOptions.None))
                    End If

                    '2025
                    .AddWithValue("GastrointestinalOptions_Notes", VerifyStringNull(peSec.GastrointestinalNotes))

                    If peSec.GastrointestinalOptions IsNot Nothing Then
                        .AddWithValue("GastrointestinalOptions_GoodDentation", VerifyBooleanNull(peSec.GastrointestinalOptions.GoodDentation))
                        .AddWithValue("GastrointestinalOptions_PoorDentation", VerifyBooleanNull(peSec.GastrointestinalOptions.PoorDentation))
                        .AddWithValue("GastrointestinalOptions_HardToPalpation", VerifyBooleanNull(peSec.GastrointestinalOptions.HardToPalpation))
                        .AddWithValue("GastrointestinalOptions_SoftToPalpation", VerifyBooleanNull(peSec.GastrointestinalOptions.SoftToPalpation))
                        .AddWithValue("GastrointestinalOptions_Tenderness", VerifyBooleanNull(peSec.GastrointestinalOptions.Tenderness))
                        .AddWithValue("GastrointestinalOptions_Visceromegaly", VerifyBooleanNull(peSec.GastrointestinalOptions.Visceromegaly))
                        .AddWithValue("GastrointestinalOptions_WNL", VerifyBooleanNull(peSec.GastrointestinalOptions.WNL))
                    End If

                    '2025
                    .AddWithValue("GenitourinaryOptions_Notes", VerifyStringNull(peSec.GenitourinaryNotes))

                    If peSec.GenitourinaryOptions IsNot Nothing Then
                        .AddWithValue("GenitourinaryOptions_DeferedGeneralAppereance", VerifyBooleanNull(peSec.GenitourinaryOptions.DeferedGeneralAppereance))
                        .AddWithValue("GenitourinaryOptions_WhithinNormalLimits", VerifyBooleanNull(peSec.GenitourinaryOptions.WhithinNormalLimits))
                    End If

                    .AddWithValue("NeckNotes", VerifyStringNull(peSec.NeckNotes))

                    If peSec.NeckOptions IsNot Nothing Then
                        .AddWithValue("NeckOptions_Masses", VerifyBooleanNull(peSec.NeckOptions.Masses))
                        .AddWithValue("NeckOptions_OverallAppearance", VerifyBooleanNull(peSec.NeckOptions.OverallAppearance))
                        .AddWithValue("NeckOptions_Symmetry", VerifyBooleanNull(peSec.NeckOptions.Symmetry))
                        .AddWithValue("NeckOptions_NormalTrachelPosition", VerifyBooleanNull(peSec.NeckOptions.NormalTrachealPosition))
                        .AddWithValue("NeckOptions_Tracheostomy", VerifyBooleanNull(peSec.NeckOptions.Tracheostomy))
                        .AddWithValue("NeckOptions_Crepitus", VerifyBooleanNull(peSec.NeckOptions.Crepitus))
                        .AddWithValue("NeckOptions_ThyroidEnlargement", VerifyBooleanNull(peSec.NeckOptions.ThyroidEnlargement))
                        .AddWithValue("NeckOptions_ThyroidTenderness", VerifyBooleanNull(peSec.NeckOptions.ThyroidTenderness))
                        .AddWithValue("NeckOptions_ThyroidMass", VerifyBooleanNull(peSec.NeckOptions.ThyroidMass))
                        .AddWithValue("NeckOptions_WNL", VerifyBooleanNull(peSec.NeckOptions.WNL))

                        .AddWithValue("NeckOption_Rigity", VerifyBooleanNull(peSec.NeckOptions.NeckOption_Rigity))
                        .AddWithValue("NeckOption_MovementLimitation", VerifyBooleanNull(peSec.NeckOptions.NeckOption_MovementLimitation))
                        .AddWithValue("NeckOption_Crackle", VerifyBooleanNull(peSec.NeckOptions.NeckOption_Crackle))
                    End If

                    .AddWithValue("ChestNotes", VerifyStringNull(peSec.ChestNotes))

                    If peSec.ChestOption IsNot Nothing Then
                        .AddWithValue("ChestOptions_IntercostalRetractions", VerifyBooleanNull(peSec.ChestOption.IntercostalRetractions))
                        .AddWithValue("ChestOptions_UseOfAccesoryMuscles", VerifyBooleanNull(peSec.ChestOption.UseOfAccesoryMuscles))
                        .AddWithValue("ChestOptions_DiaphragmaticMovement", VerifyBooleanNull(peSec.ChestOption.DiaphragmaticMovement))
                        .AddWithValue("ChestOptions_Dullness", VerifyBooleanNull(peSec.ChestOption.Dullness))
                        .AddWithValue("ChestOptions_Flatness", VerifyBooleanNull(peSec.ChestOption.Flatness))
                        .AddWithValue("ChestOptions_Hyperresonance", VerifyBooleanNull(peSec.ChestOption.Hyperresonance))
                        .AddWithValue("ChestOptions_TactileFremitus", VerifyBooleanNull(peSec.ChestOption.TactileFremitus))
                        .AddWithValue("ChestOptions_NormalBreathSounds", VerifyBooleanNull(peSec.ChestOption.NormalBreathSounds))
                        .AddWithValue("ChestOptions_AdventitiousSounds", VerifyBooleanNull(peSec.ChestOption.AdventitiousSounds))
                        .AddWithValue("ChestOptions_Rubs", VerifyBooleanNull(peSec.ChestOption.Rubs))
                        .AddWithValue("ChestOptions_Crackels", VerifyBooleanNull(peSec.ChestOption.Crackels))
                        .AddWithValue("ChestOptions_WheezingsSymmetryBreasts", VerifyBooleanNull(peSec.ChestOption.WheezingSymmetryBreasts))
                        .AddWithValue("ChestOptions_NippleDischargeBreastsMassesLumps", VerifyBooleanNull(peSec.ChestOption.NippleDischargeBreastsMassesLumps))
                        .AddWithValue("ChestOptions_BreastsTenderness", VerifyBooleanNull(peSec.ChestOption.BreastsTenderness))
                        .AddWithValue("ChestOptions_WNL", VerifyBooleanNull(peSec.ChestOption.WNL))
                        .AddWithValue("ChestOptions_BreastsMasses", VerifyBooleanNull(peSec.ChestOption.BreastsMasses))
                        .AddWithValue("ChestOptions_NippleDischarge", VerifyBooleanNull(peSec.ChestOption.NippleDischarge))
                        .AddWithValue("ChestOptions_RTFootToeAmputation", VerifyBooleanNull(peSec.ChestOption.RTFootToeAmputation))
                        .AddWithValue("ChestOptions_LTFootToeAmputation", VerifyBooleanNull(peSec.ChestOption.LTFootToeAmputation))

                    End If

                    .AddWithValue("CardiovascularNotes", VerifyStringNull(peSec.CardiovascularNotes))

                    If peSec.CardiovascularOption IsNot Nothing Then
                        .AddWithValue("CardiovascularOptions_AbnormalHeartSound", VerifyBooleanNull(peSec.CardiovascularOption.AbnormalHeartSound))
                        .AddWithValue("CardiovascularOptions_MurmursDecreasedPedalPulses", VerifyBooleanNull(peSec.CardiovascularOption.MurmursDecreasedPedalPulses))
                        .AddWithValue("CardiovascularOptions_LegEdema", VerifyBooleanNull(peSec.CardiovascularOption.LegEdema))
                        .AddWithValue("CardiovascularOptions_Varicosities", VerifyBooleanNull(peSec.CardiovascularOption.Varicosities))
                        .AddWithValue("CardiovascularOptions_AbnormalTemperature", VerifyBooleanNull(peSec.CardiovascularOption.AbnormalTemperature))
                        .AddWithValue("CardiovascularOptions_WNL", VerifyBooleanNull(peSec.CardiovascularOption.WNL))
                        .AddWithValue("CardiovascularOptions_DecreasedPedalPulses", VerifyBooleanNull(peSec.CardiovascularOption.DecreasedPedalPulses))

                        '2025
                        .AddWithValue("CardiovascularOptions_RegularRateRhytm", VerifyBooleanNull(peSec.CardiovascularOption.RegularRateRhytm))
                        .AddWithValue("CardiovascularOptions_IrregularRateRhytm", VerifyBooleanNull(peSec.CardiovascularOption.IrregularRateRhytm))
                        .AddWithValue("CardiovascularOptions_Murmurs", VerifyBooleanNull(peSec.CardiovascularOption.Murmurs))
                        .AddWithValue("CardiovascularOptions_Gallops", VerifyBooleanNull(peSec.CardiovascularOption.Gallops))
                        .AddWithValue("CardiovascularOptions_Rubs", VerifyBooleanNull(peSec.CardiovascularOption.Rubs))
                        .AddWithValue("CardiovascularOptions_PainUponPrecordialPalpation", VerifyBooleanNull(peSec.CardiovascularOption.PainUponPrecordialPalpation))
                        .AddWithValue("CardiovascularOptions_Other", VerifyBooleanNull(peSec.CardiovascularOption.Other))
                        .AddWithValue("CardiovascularOptions_None", VerifyBooleanNull(peSec.CardiovascularOption.None))
                    End If

                    '**** Amputacion ******
                    If peSec.AmputationLegRT_BKA IsNot Nothing Then .AddWithValue("AmputationLegRT_BKA", VerifyBooleanNull(peSec.AmputationLegRT_BKA))
                    If peSec.AmputationLegRT_AKA IsNot Nothing Then .AddWithValue("AmputationLegRT_AKA", VerifyBooleanNull(peSec.AmputationLegRT_AKA))
                    If peSec.AmputationLegRT_Toe IsNot Nothing Then .AddWithValue("AmputationLegRT_Toe", VerifyBooleanNull(peSec.AmputationLegRT_Toe))

                    If peSec.AmputationLegLT_BKA IsNot Nothing Then .AddWithValue("AmputationLegLT_BKA", VerifyBooleanNull(peSec.AmputationLegLT_BKA))
                    If peSec.AmputationLegLT_AKA IsNot Nothing Then .AddWithValue("AmputationLegLT_AKA", VerifyBooleanNull(peSec.AmputationLegLT_AKA))
                    If peSec.AmputationLegLT_Toe IsNot Nothing Then .AddWithValue("AmputationLegLT_Toe", VerifyBooleanNull(peSec.AmputationLegLT_Toe))


                    .AddWithValue("AbdomenNotes", VerifyStringNull(peSec.AbdomenNotes))

                    If peSec.AbdomenOptions IsNot Nothing Then
                        .AddWithValue("AbdomenOptions_Masses", VerifyBooleanNull(peSec.AbdomenOptions.Masses))
                        .AddWithValue("AbdomenOptions_Tenderness", VerifyBooleanNull(peSec.AbdomenOptions.Tenderness))
                        .AddWithValue("AbdomenOptions_Hernia", VerifyBooleanNull(peSec.AbdomenOptions.Hernia))
                        .AddWithValue("AbdomenOptions_LiverEnlargement", VerifyBooleanNull(peSec.AbdomenOptions.LiverEnlargement))
                        .AddWithValue("AbdomenOptions_SpleenEnlargement", VerifyBooleanNull(peSec.AbdomenOptions.SpleenEnlargement))
                        .AddWithValue("AbdomenOptions_Colostomy", VerifyBooleanNull(peSec.AbdomenOptions.Colostomy))
                        .AddWithValue("AbdomenOptions_Ileostomy", VerifyBooleanNull(peSec.AbdomenOptions.Ileostomy))
                        .AddWithValue("AbdomenOptions_Gastrostomy", VerifyBooleanNull(peSec.AbdomenOptions.Gastrostomy))
                        .AddWithValue("AbdomenOptions_WNL", VerifyBooleanNull(peSec.AbdomenOptions.WNL))
                        If peSec.AbdomenOptions.Cystostomy IsNot Nothing Then .AddWithValue("AbdomenOptions_Cystostomy", VerifyBooleanNull(peSec.AbdomenOptions.Cystostomy))
                        If peSec.AbdomenOptions.AbdomenOptions_UmbilicalInfection IsNot Nothing Then .AddWithValue("AbdomenOptions_UmbilicalInfection", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_UmbilicalInfection))
                        If peSec.AbdomenOptions.AbdomenOptions_Distention IsNot Nothing Then .AddWithValue("AbdomenOptions_Distention", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_Distention))
                        If peSec.AbdomenOptions.AbdomenOptions_Constipation IsNot Nothing Then .AddWithValue("AbdomenOptions_Constipation", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_Constipation))
                        If peSec.AbdomenOptions.AbdomenOptions_Colics IsNot Nothing Then .AddWithValue("AbdomenOptions_Colics", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_Colics))
                        If peSec.AbdomenOptions.AbdomenOptions_Reflux IsNot Nothing Then .AddWithValue("AbdomenOptions_Reflux", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_Reflux))

                        If peSec.AbdomenOptions.AbdomenOptions_Rebound IsNot Nothing Then .AddWithValue("AbdomenOptions_Rebound", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_Rebound))
                        If peSec.AbdomenOptions.AbdomenOptions_Guarding IsNot Nothing Then .AddWithValue("AbdomenOptions_Guarding", VerifyBooleanNull(peSec.AbdomenOptions.AbdomenOptions_Guarding))

                    End If

                    .AddWithValue("GenitaliaNotes", VerifyStringNull(peSec.GenitaliaGroinButtocksNotes))

                    If peSec.GenitaliaGroinButtocksNotesOptions IsNot Nothing Then
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_DefferedGeneralAppearance", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.DefferedGeneralAppearance))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_HairDistribution", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.HairDistribution))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Lesions", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Lesions))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Cyst", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Cyst))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Rashes", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Rashes))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Size", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Size))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Symmetry", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Symmetry))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Masses", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Masses))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Discharge", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Discharge))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Scarring", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Scarring))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Deformities", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Deformities))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Nodularity", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Nodularity))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Tenderness", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Tenderness))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Enlargement", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Enlargement))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Hemorrhoids", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Hemorrhoids))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Prolapse", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Prolapse))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_EstrogenEffect", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.EstrogenEffect))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_PelvicSupport", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.PelvicSupport))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Cystocele", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Cystocele))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Rectocele", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Rectocele))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_WNL", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.WNL))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_Urostomy", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.Urostomy))
                        .AddWithValue("GenitaliaGroinButtocksNotesOptions_FoleyCatheterUse", VerifyBooleanNull(peSec.GenitaliaGroinButtocksNotesOptions.FoleyCatheterUse))



                    End If


                    .AddWithValue("MusculokeletalNotes", VerifyStringNull(peSec.MusculoskeletalNotes))

                    If peSec.MusculoskeletalOptions IsNot Nothing Then
                        .AddWithValue("MusculokeletalOptions_AbnormalGait", VerifyBooleanNull(peSec.MusculoskeletalOptions.AbnormalGait))
                        .AddWithValue("MusculokeletalOptions_ClubbingNails", VerifyBooleanNull(peSec.MusculoskeletalOptions.ClubbingNails))
                        .AddWithValue("MusculokeletalOptions_CyanosisDigits", VerifyBooleanNull(peSec.MusculoskeletalOptions.CyanosisDigits))
                        .AddWithValue("MusculokeletalOptions_UpperExtremitiesAsymmetry", VerifyBooleanNull(peSec.MusculoskeletalOptions.UpperExtremitiesAsymmetry))
                        .AddWithValue("MusculokeletalOptions_LowerExtremitiesAsymmetry", VerifyBooleanNull(peSec.MusculoskeletalOptions.LowerExtremitiesAsymmetry))
                        .AddWithValue("MusculokeletalOptions_Dislocation", VerifyBooleanNull(peSec.MusculoskeletalOptions.Dislocation))
                        .AddWithValue("MusculokeletalOptions_DislocationNotes", VerifyStringNull(peSec.MusculoskeletalOptions.DislocationNotes))
                        .AddWithValue("MusculokeletalOptions_AbnormalMuscleStrengthTone", VerifyBooleanNull(peSec.MusculoskeletalOptions.AbnormalMuscleStrengthTone))
                        .AddWithValue("MusculokeletalOptions_Flaccid", VerifyBooleanNull(peSec.MusculoskeletalOptions.Flaccid))
                        .AddWithValue("MusculokeletalOptions_CogWheel", VerifyBooleanNull(peSec.MusculoskeletalOptions.CogWheel))
                        .AddWithValue("MusculokeletalOptions_Spastic", VerifyBooleanNull(peSec.MusculoskeletalOptions.Spastic))
                        .AddWithValue("MusculokeletalOptions_AbnormalMovements", VerifyBooleanNull(peSec.MusculoskeletalOptions.AbnormalMovements))
                        .AddWithValue("MusculokeletalOptions_WNL", VerifyBooleanNull(peSec.MusculoskeletalOptions.WNL))

                        '2025
                        .AddWithValue("MusculoskeletalOption_NoDeformitiesOrDeformations", VerifyBooleanNull(peSec.MusculoskeletalOptions.NoDeformitiesOrDeformations))
                        .AddWithValue("MusculoskeletalOption_NormalGait", VerifyBooleanNull(peSec.MusculoskeletalOptions.NormalGait))
                        .AddWithValue("MusculoskeletalOption_AdequateROM", VerifyBooleanNull(peSec.MusculoskeletalOptions.AdequateROM))
                        .AddWithValue("MusculoskeletalOption_InadequateROM", VerifyBooleanNull(peSec.MusculoskeletalOptions.InadequateROM))
                    End If

                    .AddWithValue("SkinNotes", VerifyStringNull(peSec.SkinNotes))

                    If peSec.SkinOptions IsNot Nothing Then
                        .AddWithValue("SkinOptions_Rashes", VerifyBooleanNull(peSec.SkinOptions.Rashes))
                        .AddWithValue("SkinOptions_Lesions", VerifyBooleanNull(peSec.SkinOptions.Lesions))
                        .AddWithValue("SkinOptions_Ulcers", VerifyBooleanNull(peSec.SkinOptions.Ulcers))
                        .AddWithValue("SkinOptions_Nodules", VerifyBooleanNull(peSec.SkinOptions.Nodules))
                        .AddWithValue("SkinOptions_Induration", VerifyBooleanNull(peSec.SkinOptions.Induration))
                        .AddWithValue("SkinOptions_Tightening", VerifyBooleanNull(peSec.SkinOptions.Tightening))
                        .AddWithValue("SkinOptions_WNL", VerifyBooleanNull(peSec.SkinOptions.WNL))
                        .AddWithValue("SkinOptions_PurpuricLesionsNoted", VerifyBooleanNull(peSec.SkinOptions.PurpuricLesionsNoted))
                    End If

                    .AddWithValue("PsychiatricNotes", VerifyStringNull(peSec.PsychiatricNeurologicNotes))

                    If peSec.PsychiatricNeurologicOptions IsNot Nothing Then
                        .AddWithValue("PsychiatricNeurologicOptions_CranialNervesWithDeficits", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.CranialNervesWithDeficits))
                        .AddWithValue("PsychiatricNeurologicOptions_Babinsky", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.Babinsky))
                        .AddWithValue("PsychiatricNeurologicOptions_SentationByTouch", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.SensationByTouch))
                        .AddWithValue("PsychiatricNeurologicOptions_NoSensationTouchLegs", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.NoSensationTouchLegs))
                        .AddWithValue("PsychiatricNeurologicOptions_OrientedToTime", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.OrientedToTime))
                        .AddWithValue("PsychiatricNeurologicOptions_PlaceAndPerson", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.PlaceAndPerson))
                        .AddWithValue("PsychiatricNeurologicOptions_DepressedMode", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.DepressedMode))
                        .AddWithValue("PsychiatricNeurologicOptions_Anxiety", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.Anxiety))
                        .AddWithValue("PsychiatricNeurologicOptions_Agitation", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.Agitation))
                        .AddWithValue("PsychiatricNeurologicOptions_WNL", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.WNL))
                        .AddWithValue("PsychiatricNeurologicOptions_Hemiplejia", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.Hemiplejia))
                        .AddWithValue("PsychiatricNeurologicOptions_Cuadriplejia", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.Cuadriplejia))
                        .AddWithValue("PsychiatricNeurologicOptions_Paraplejia", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.Paraplejia))

                        '2025
                        .AddWithValue("NeurologicOption_AmbulatingWOLimitation", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.AmbulatingWOLimitation))
                        .AddWithValue("NeurologicOption_NormalMuscleStrengthTone", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.NormalMuscleStrengthTone))
                        .AddWithValue("NeurologicOption_AbnormalMuscleStrengthTone", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.AbnormalMuscleStrengthTone))
                        .AddWithValue("NeurologicOption_FocalDeficits", VerifyBooleanNull(peSec.PsychiatricNeurologicOptions.FocalDeficits))

                    End If

                    .AddWithValue("HemotalogicNotes", VerifyStringNull(peSec.HematologicLymphaticImmunologicNotes))

                    If peSec.HematologicLymphaticImmunologicOptions IsNot Nothing Then
                        .AddWithValue("HematologicLymphaticImmunologicOptions_LymphNodes", VerifyBooleanNull(peSec.HematologicLymphaticImmunologicOptions.LymphNodes))
                        .AddWithValue("HematologicLymphaticImmunologicOptions_LymphNodesNotes", VerifyStringNull(peSec.HematologicLymphaticImmunologicOptions.LymphNodesNotes))
                        .AddWithValue("HematologicLymphaticImmunologicOptions_WNL", VerifyBooleanNull(peSec.HematologicLymphaticImmunologicOptions.WNL))
                    End If

                    .AddWithValue("HeadCircumference", VerifyDecimalNull(peSec.HeadCircumference))
                    .AddWithValue("PercentilWT", VerifyDecimalNull(peSec.PercentilWT))
                    .AddWithValue("PercentilHT", VerifyDecimalNull(peSec.PercentilHT))
                    .AddWithValue("PercentilHead", VerifyDecimalNull(peSec.PercentilHead))

                End With

                dbo.ExecuteCommand(cmd)

            End If

        Catch ex As Exception
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveMyocardialInfarction(ByVal claimKey As Long, ByVal myocardialSec As MyocardialInfarctionSection,
                                         ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If myocardialSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMyocardialInfarction"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    If myocardialSec.OldMI IsNot Nothing Then .AddWithValue("OldMi", VerifyBooleanNull(myocardialSec.OldMI))
                    If myocardialSec.BetaBlocker IsNot Nothing Then .AddWithValue("BetaBlocker", VerifyBooleanNull(myocardialSec.BetaBlocker))
                    If myocardialSec.BetaBlockerType IsNot Nothing Then .AddWithValue("BetaBlockerType", VerifyStringNull(myocardialSec.BetaBlockerType))
                    If myocardialSec.OtherTreatmentCircumstances IsNot Nothing Then .AddWithValue("OldMIOtherTreatment", VerifyStringNull(myocardialSec.OtherTreatmentCircumstances))
                    If myocardialSec.AMI6Months IsNot Nothing Then .AddWithValue("MedicalHistory_AMI_6_Months", VerifyBooleanNull(myocardialSec.AMI6Months))
                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveBMIAssociatedDiagnoses(ByVal claimKey As Long, ByVal bmiAssocciateDxSec As BMIAssociatedDiagnosesSection,
                                         ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If bmiAssocciateDxSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveBMIAssociatedDiagnoses"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)

                    .AddWithValue("NA", bmiAssocciateDxSec.NA)
                    If bmiAssocciateDxSec.Obesity IsNot Nothing Then .AddWithValue("Obesity", VerifyBooleanNull(bmiAssocciateDxSec.Obesity))
                    If bmiAssocciateDxSec.MorbidObesity IsNot Nothing Then .AddWithValue("MorbidObesity", VerifyBooleanNull(bmiAssocciateDxSec.MorbidObesity))
                    If bmiAssocciateDxSec.Malnutrition IsNot Nothing Then .AddWithValue("Malnutrition", VerifyBooleanNull(bmiAssocciateDxSec.Malnutrition))
                    If bmiAssocciateDxSec.EvaluationTreatmentPlan IsNot Nothing Then .AddWithValue("BMIPlanTreatment", VerifyStringNull(bmiAssocciateDxSec.EvaluationTreatmentPlan))
                    If bmiAssocciateDxSec.MalnutritionGradeTypeText IsNot Nothing Then .AddWithValue("MalnutritionGradeTypeText", VerifyStringNull(bmiAssocciateDxSec.MalnutritionGradeTypeText))

                    If bmiAssocciateDxSec.DeficiencyBComplex IsNot Nothing Then .AddWithValue("DeficiencyBComplex", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyBComplex))
                    If bmiAssocciateDxSec.DeficiencyVitaminB12 IsNot Nothing Then .AddWithValue("DeficiencyVitaminB12", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyVitaminB12))
                    If bmiAssocciateDxSec.DeficiencyVitaminB6 IsNot Nothing Then .AddWithValue("DeficiencyVitaminB6", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyVitaminB6))
                    If bmiAssocciateDxSec.DeficiencyOtherVitaminNutrients IsNot Nothing Then .AddWithValue("DeficiencyOtherVitaminNutrients", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyOtherVitaminNutrients))
                    If bmiAssocciateDxSec.DeficiencyOtherVitaminNutrientsComments IsNot Nothing Then .AddWithValue("DeficiencyOtherVitaminNutrientsComments", VerifyStringNull(bmiAssocciateDxSec.DeficiencyOtherVitaminNutrientsComments))
                    If bmiAssocciateDxSec.MalnutritionScreeningAssesment IsNot Nothing Then .AddWithValue("MalnutritionScreeningAssesment", VerifyStringNull(bmiAssocciateDxSec.MalnutritionScreeningAssesment))

                End With

                dbo.ExecuteCommand(cmd)
                If bmiAssocciateDxSec IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "nutrition", bmiAssocciateDxSec.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If
            Else
                bmiAssocciateDxSec = New BMIAssociatedDiagnosesSection()
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveBMIAssociatedDiagnoses"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NA", bmiAssocciateDxSec.NA)

                    If bmiAssocciateDxSec.Obesity IsNot Nothing Then .AddWithValue("Obesity", VerifyBooleanNull(bmiAssocciateDxSec.Obesity))
                    If bmiAssocciateDxSec.MorbidObesity IsNot Nothing Then .AddWithValue("MorbidObesity", VerifyBooleanNull(bmiAssocciateDxSec.MorbidObesity))
                    If bmiAssocciateDxSec.Malnutrition IsNot Nothing Then .AddWithValue("Malnutrition", VerifyBooleanNull(bmiAssocciateDxSec.Malnutrition))
                    If bmiAssocciateDxSec.EvaluationTreatmentPlan IsNot Nothing Then .AddWithValue("BMIPlanTreatment", VerifyStringNull(bmiAssocciateDxSec.EvaluationTreatmentPlan))
                    If bmiAssocciateDxSec.MalnutritionGradeTypeText IsNot Nothing Then .AddWithValue("MalnutritionGradeTypeText", VerifyStringNull(bmiAssocciateDxSec.MalnutritionGradeTypeText))
                    If bmiAssocciateDxSec.DeficiencyBComplex IsNot Nothing Then .AddWithValue("DeficiencyBComplex", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyBComplex))
                    If bmiAssocciateDxSec.DeficiencyVitaminB12 IsNot Nothing Then .AddWithValue("DeficiencyVitaminB12", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyVitaminB12))
                    If bmiAssocciateDxSec.DeficiencyVitaminB6 IsNot Nothing Then .AddWithValue("DeficiencyVitaminB6", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyVitaminB6))
                    If bmiAssocciateDxSec.DeficiencyOtherVitaminNutrients IsNot Nothing Then .AddWithValue("DeficiencyOtherVitaminNutrients", VerifyBooleanNull(bmiAssocciateDxSec.DeficiencyOtherVitaminNutrients))
                    If bmiAssocciateDxSec.DeficiencyOtherVitaminNutrientsComments IsNot Nothing Then .AddWithValue("DeficiencyOtherVitaminNutrientsComments", VerifyStringNull(bmiAssocciateDxSec.DeficiencyOtherVitaminNutrientsComments))
                    If bmiAssocciateDxSec.MalnutritionScreeningAssesment IsNot Nothing Then .AddWithValue("MalnutritionScreeningAssesment", VerifyStringNull(bmiAssocciateDxSec.MalnutritionScreeningAssesment))

                End With

                dbo.ExecuteCommand(cmd)

                If bmiAssocciateDxSec IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "nutrition", bmiAssocciateDxSec.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveDepressionInventorySection(ByVal claimKey As Long, ByVal diSec As DepressionInventorySection,
                                                ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If diSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveDepressionInventory"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("SadLevel", VerifyNull(diSec.Choose1.Value))
                    .AddWithValue("LostInterestLevel", VerifyNull(diSec.Choose2.Value))
                    .AddWithValue("LackEnergyLevel", VerifyNull(diSec.Choose3.Value))
                    .AddWithValue("ConfidentLevel", VerifyNull(diSec.Choose4.Value))
                    .AddWithValue("GuiltLevel", VerifyNull(diSec.Choose5.Value))
                    .AddWithValue("LifeInterest", VerifyNull(diSec.Choose6.Value))
                    .AddWithValue("ConcentrationLevel", VerifyNull(diSec.Choose7.Value))
                    .AddWithValue("RestlesLevel", VerifyNull(diSec.Choose8a.Value))
                    .AddWithValue("SubduedLevel", VerifyNull(diSec.Choose8b.Value))
                    .AddWithValue("SleepLevel", VerifyNull(diSec.Choose9.Value))
                    .AddWithValue("ReduceAppetiteLevel", VerifyNull(diSec.Choose10a.Value))
                    .AddWithValue("IncreaseAppetiteLevel", VerifyNull(diSec.Choose10b.Value))
                    .AddWithValue("IsMild", VerifyBooleanNull(diSec.IsMild.Value))
                    .AddWithValue("IsSevere", VerifyBooleanNull(diSec.IsSevere.Value))
                    .AddWithValue("IsMajor", VerifyBooleanNull(diSec.IsMajor.Value))
                    .AddWithValue("IsModerate", VerifyBooleanNull(diSec.IsModerate.Value))
                    .AddWithValue("PlanOfTreatment", VerifyStringNull(diSec.PlanOfTreatment.Value))
                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch 'ex As Exception
            Throw
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveDMEUseSection(ByVal claimKey As Long, ByVal dmeSec As DMEUseSection,
                                    ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If dmeSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveDME"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("UsingOxygen", VerifyBooleanNull(dmeSec.UsingOxygen.Value))
                    .AddWithValue("DueToHypoxiaInAir", VerifyBooleanNull(dmeSec.DueToHypoxiaInAir.Value))
                    .AddWithValue("CPAP", VerifyBooleanNull(dmeSec.CPAP.Value))
                    .AddWithValue("AboveKneeProsthesis", VerifyBooleanNull(dmeSec.AboveKneeProsthesis.Value))
                    .AddWithValue("BelowKneeProsthesis", VerifyBooleanNull(dmeSec.BelowKneeProsthesis.Value))
                    .AddWithValue("HasSuppliesNeeded", VerifyBooleanNull(dmeSec.HasSuppliesNeeded.Value))
                    .AddWithValue("Gastrostomy", VerifyBooleanNull(dmeSec.Gastrostomy.Value))
                    .AddWithValue("Colostomy", VerifyBooleanNull(dmeSec.Colostomy.Value))
                    .AddWithValue("Urostomy", VerifyBooleanNull(dmeSec.Urostomy.Value))
                    .AddWithValue("Tracheostomy", VerifyBooleanNull(dmeSec.Tracheostomy.Value))
                    .AddWithValue("UsingWheelchair", VerifyBooleanNull(dmeSec.UsingWheelchair.Value))
                    .AddWithValue("UsingWheelchairReason", VerifyStringNull(dmeSec.UsingWheelchairReason.Value))
                    .AddWithValue("Comments", VerifyStringNull(dmeSec.Comments.Value))
                End With

                dbo.ExecuteCommand(cmd)

            End If

        Catch 'ex As Exception
            Throw
        Finally
            cmd.Dispose()
        End Try

    End Sub

#End Region


#Region "SaveAhaShort"


    Private Sub SaveShortForm(ByVal claimKey As Long, ByVal stSecHeader As FormHeaderSection, ByVal stSec As CovidSection,
                                  ByVal dbo As SqlDataObject)



        Try
            Dim cmd As New SqlClient.SqlCommand

            Try

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveAhaShort"
                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                End With


                If stSecHeader IsNot Nothing Then
                    With cmd.Parameters
                        .AddWithValue("Header_ProviderLocation", stSecHeader.ProviderLocation.Value)
                        .AddWithValue("Header_PatientLocation", stSecHeader.PatientLocation.Value)

                    End With
                End If

                If stSec IsNot Nothing Then
                    With cmd.Parameters
                        .AddWithValue("Covid_TripInTheLast14", stSec.TripInTheLast14?.Value)
                        .AddWithValue("Covid_TripInTheLast14Description", stSec.TripInTheLast14Description?.Value)
                        .AddWithValue("Covid_CovidPositive", stSec.CovidPositive?.Value)
                        .AddWithValue("Covid_ContactCovidPositive", stSec.ContactCovidPositive?.Value)
                        .AddWithValue("Covid_CovidNegative", stSec.CovidNegative?.Value)
                        .AddWithValue("Covid_OtherMedicalHistoryDescription", stSec.OtherMedicalHistoryDescription?.Value)

                    End With
                End If


                dbo.ExecuteCommand(cmd)



            Catch ex As Exception
                Throw
            Finally
                cmd.Dispose()
            End Try

        Catch ex As Exception
            Throw ex
        End Try

    End Sub



#End Region

    Private Sub SaveRheumatoidArthritis(ByVal claimKey As Long, ByVal raSec As RheumatoidArthritisSection,
                                        ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If IsNothing(raSec) Then
                raSec = New RheumatoidArthritisSection()
            End If

            If raSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveRheumatoidArthritis"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NA", raSec.NA)
                    .AddWithValue("RANoManifestations", VerifyBooleanNull(raSec.RANoManifestations))
                    .AddWithValue("RAWithPolyneuropathy", VerifyBooleanNull(raSec.RAWithPolyneuropathy))
                    .AddWithValue("RAWithMyopathy", VerifyBooleanNull(raSec.RAWithMyopathy))
                    .AddWithValue("RAOtherManifestations", VerifyStringNull(raSec.RAOtherManifestations))
                    .AddWithValue("DMARDs", VerifyBooleanNull(raSec.DMARDs))
                    .AddWithValue("DMARDsSpecify", VerifyStringNull(raSec.DMARDsSpecify))
                    .AddWithValue("PtRefuses", VerifyBooleanNull(raSec.PtRefuese))
                    .AddWithValue("OtherTreatmentConditions", VerifyStringNull(raSec.OtherTreatmentConditions))

                    .AddWithValue("Arthritis", VerifyBooleanNull(raSec.Arthritis))
                    .AddWithValue("ArthritisLocationType", VerifyStringNull(raSec.ArthritisLocationType))
                    .AddWithValue("NSAIDS", VerifyBooleanNull(raSec.NSAIDS))
                    .AddWithValue("NSAIDSOtherTreatment", VerifyStringNull(raSec.NSAIDSOtherTreatment))
                    .AddWithValue("AffectedJoints", VerifyStringNull(raSec.AffectedJoints))
                    .AddWithValue("InflammatoryPolyarthritis", VerifyBooleanNull(raSec.InflammatoryPolyarthritis))
                    .AddWithValue("InflammatoryPolyarthritisComments", VerifyStringNull(raSec.InflammatoryPolyarthritisComments))
                    .AddWithValue("ArthropathySequelaViralInfection", VerifyBooleanNull(raSec.ArthropathySequelaViralInfection))
                    .AddWithValue("ArthropathySequelaViralInfectionComments", VerifyStringNull(raSec.ArthropathySequelaViralInfectionComments))
                    .AddWithValue("ArtritisPsoriatrica", VerifyBooleanNull(raSec.RheumatoidArthritis_ArtritisPsoriatrica))
                    .AddWithValue("Osteoartritis", VerifyBooleanNull(raSec.RheumatoidArthritis_Osteoartritis))
                    .AddWithValue("ArtritisPsoriatricaComment", VerifyStringNull(raSec.RheumatoidArthritis_ArtritisPsoriatricaComment))
                    .AddWithValue("OsteoartritisComment", VerifyStringNull(raSec.RheumatoidArthritis_OsteoartritisComment))
                    .AddWithValue("RAOtherManifestationsCheckBox", VerifyBooleanNull(raSec.RAOtherManifestationsCheckBox))
                    'ADD 2026 Changes '
                    .AddWithValue("Osteoporosis", VerifyBooleanNull(raSec.Osteoporosis))
                    .AddWithValue("Osteopenia", VerifyBooleanNull(raSec.Osteopenia))
                    .AddWithValue("OsteoTreatmentPlan", VerifyStringNull(raSec.OsteoTreatmentPlan))





                End With

                dbo.ExecuteCommand(cmd)
                If raSec IsNot Nothing AndAlso raSec.MedicationList IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "RheumatoidArthritis", raSec.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If
            End If

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveAssessmentPlanTreatment(ByVal claimKey As Long, ByVal asptSec As AssessmentPlanOfTreatmentSection,
                                            ByVal isDiabetic As BooleanField, ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If asptSec Is Nothing Then
                asptSec = New AssessmentPlanOfTreatmentSection()
            End If

            If asptSec IsNot Nothing Then
                If Not IsNothing(asptSec.No) Then
                    If (Not IsNothing(asptSec.No.Value) AndAlso asptSec.No.Value) Then
                        With asptSec

                            '.No.Value = False
                            .DMType = Nothing
                            .DMSecondary = Nothing
                            .Controlled = Nothing
                            .PoorlyController = Nothing
                            .DMComments = Nothing
                            .DiabeticNeuropathy = Nothing
                            .DiabeticPVD = Nothing
                            .DiabeticNephropathy = Nothing
                            '.DiabeticCKDStage = Nothing
                            '.DMWithOphtalmicManifestations = ""
                            If .OtherDiabeticComplication IsNot Nothing Then .OtherDiabeticComplication.Value = ""
                            '.PlanOfTreatment = ""
                            '.InsulinUsage = Nothing

                            .DiabeticNeuropathyComments = Nothing
                            .DiabeticPVDComments = Nothing
                            .DiabeticNephropathyComments = Nothing

                            If .DiabeticCataracts IsNot Nothing Then .DiabeticCataracts.Value = Nothing
                            If .DiabeticCataractsComments IsNot Nothing Then .DiabeticCataractsComments.Value = Nothing

                            If .OtherDiabeticComplicationComments IsNot Nothing Then .OtherDiabeticComplicationComments.Value = ""

                            If .Retinopathy IsNot Nothing Then .Retinopathy.Value = Nothing
                            If .RetinopathyComments IsNot Nothing Then .RetinopathyComments.Value = Nothing

                            If .Proliferative IsNot Nothing Then .Proliferative.Value = Nothing
                            If .ProliferativeComments IsNot Nothing Then .ProliferativeComments.Value = Nothing

                            If .Dermatitis IsNot Nothing Then .Dermatitis.Value = Nothing
                            If .DermatitisComments IsNot Nothing Then .DermatitisComments.Value = Nothing

                            If .Periodontal IsNot Nothing Then .Periodontal.Value = Nothing
                            If .PeriodontalComments IsNot Nothing Then .PeriodontalComments.Value = Nothing

                            If .DiabeticArthropathy IsNot Nothing Then .DiabeticArthropathy.Value = Nothing
                            If .DiabeticArthropathyComment IsNot Nothing Then .DiabeticArthropathyComment.Value = Nothing

                            If .DMType Is Nothing Then .DMType = New BooleanField()



                        End With
                    Else
                        If IsNothing(asptSec.DMType) Then
                            'asptSec.DMType = Nothing
                        End If

                    End If


                End If

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveAssessmentPlanOfTreatment"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    If asptSec.No IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_No", VerifyBooleanNull(asptSec.No))
                    .AddWithValue("AssessmentPlanTreatment_DMType", VerifyBooleanNull(asptSec.DMType))
                    .AddWithValue("AssessmentPlanTreatment_Controlled", VerifyBooleanNull(asptSec.Controlled))
                    If asptSec.PoorlyController IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_PoorlyController", VerifyBooleanNull(asptSec.PoorlyController))
                    .AddWithValue("AssessmentPlanTreatment_DMComments", VerifyStringNull(asptSec.DMComments))
                    .AddWithValue("AssessmentPlanTreatment_DiabeticNeuropathy", VerifyBooleanNull(asptSec.DiabeticNeuropathy))
                    .AddWithValue("AssessmentPlanTreatment_DiabeticPVD", VerifyBooleanNull(asptSec.DiabeticPVD))
                    .AddWithValue("AssessmentPlanTreatment_DiabeticNephropathy", VerifyBooleanNull(asptSec.DiabeticNephropathy))
                    '.AddWithValue("AssessmentPlanTreatment_DiabeticCKDStage", VerifyNull(asptSec.DiabeticCKDStage.Value))
                    '.AddWithValue("AssessmentPlanTreatment_DMWithOphtalmicManifesttation", VerifyStringNull(asptSec.DMWithOphtalmicManifestations.Value))
                    .AddWithValue("AssessmentPlanTreatment_OtherComplication", VerifyStringNull(asptSec.OtherDiabeticComplication))
                    '.AddWithValue("AssessmentPlanTreatment_PlanOfTreatment", VerifyStringNull(asptSec.PlanOfTreatment.Value))
                    '.AddWithValue("AssessmentPlanTreatment_InsulinUsage", VerifyBooleanNull(asptSec.InsulinUsage.Value))
                    .AddWithValue("AssessmentPlanTreatment_DiabeticNeuropathyComments", VerifyStringNull(asptSec.DiabeticNeuropathyComments))
                    .AddWithValue("AssessmentPlanTreatment_DiabeticNephropathyComments", VerifyStringNull(asptSec.DiabeticNephropathyComments))
                    .AddWithValue("AssessmentPlanTreatment_DiabeticPVDComments", VerifyStringNull(asptSec.DiabeticPVDComments))
                    .AddWithValue("AssessmentPlanTreatment_OtherComplicationComments", VerifyStringNull(asptSec.OtherDiabeticComplicationComments))
                    If asptSec.DiabeticCataracts IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_DiabeticCataracts", VerifyBooleanNull(asptSec.DiabeticCataracts))
                    If asptSec.DiabeticCataractsComments IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_DiabeticCataractsComments", VerifyStringNull(asptSec.DiabeticCataractsComments))
                    .AddWithValue("AssessmentPlanTreatment_DMSecundary", VerifyBooleanNull(asptSec.DMSecondary))
                    If asptSec.Retinopathy IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_Retinopathy", VerifyBooleanNull(asptSec.Retinopathy))
                    If asptSec.RetinopathyComments IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_RetinopathyComments", VerifyStringNull(asptSec.RetinopathyComments))
                    If asptSec.Proliferative IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_Proliferative", VerifyBooleanNull(asptSec.Proliferative))
                    If asptSec.ProliferativeComments IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_ProliferativeComments", VerifyStringNull(asptSec.ProliferativeComments))
                    If asptSec.Dermatitis IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_Dermatitis", VerifyBooleanNull(asptSec.Dermatitis))
                    If asptSec.DermatitisComments IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_DermatitisComments", VerifyStringNull(asptSec.DermatitisComments))
                    If asptSec.Periodontal IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_Periodontal", VerifyBooleanNull(asptSec.Periodontal))
                    If asptSec.PeriodontalComments IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_PeriodontalComments", VerifyStringNull(asptSec.PeriodontalComments))

                    If asptSec.DMSecondaryText IsNot Nothing Then .AddWithValue("DMSecondaryText", VerifyStringNull(asptSec.DMSecondaryText))
                    If asptSec.OutOfControl IsNot Nothing Then .AddWithValue("OutOfControl", VerifyBooleanNull(asptSec.OutOfControl))
                    If asptSec.UncontrolledWithHyperglycemia IsNot Nothing Then .AddWithValue("UncontrolledWithHyperglycemia", VerifyBooleanNull(asptSec.UncontrolledWithHyperglycemia))
                    If asptSec.UncontrolledWithHypoglycemia IsNot Nothing Then .AddWithValue("UncontrolledWithHypoglycemia", VerifyBooleanNull(asptSec.UncontrolledWithHypoglycemia))
                    If asptSec.HyperlipidemiaDueDM IsNot Nothing Then .AddWithValue("HyperlipidemiaDueDM", VerifyBooleanNull(asptSec.HyperlipidemiaDueDM))

                    If asptSec.DMPlanAndTreatmentComments1 IsNot Nothing Then .AddWithValue("DMPlanAndTreatmentComments1", VerifyStringNull(asptSec.DMPlanAndTreatmentComments1))
                    If asptSec.DMPlanAndTreatmentComments2 IsNot Nothing Then .AddWithValue("DMPlanAndTreatmentComments2", VerifyStringNull(asptSec.DMPlanAndTreatmentComments2))
                    If asptSec.DMPlanAndTreatmentComments3 IsNot Nothing Then .AddWithValue("DMPlanAndTreatmentComments3", VerifyStringNull(asptSec.DMPlanAndTreatmentComments3))

                    If asptSec.DiabeticArthropathy IsNot Nothing Then .AddWithValue("DiabeticArthropathy", VerifyBooleanNull(asptSec.DiabeticArthropathy))
                    If asptSec.DiabeticArthropathyComment IsNot Nothing Then .AddWithValue("DiabeticArthropathyComment", VerifyStringNull(asptSec.DiabeticArthropathyComment))

                    If asptSec.GestionalDiabetes IsNot Nothing Then .AddWithValue("GestionalDiabetes", VerifyBooleanNull(asptSec.GestionalDiabetes))
                    If asptSec.GestionalDiabetesComment IsNot Nothing Then .AddWithValue("GestionalDiabetesComment", VerifyStringNull(asptSec.GestionalDiabetesComment))

                    'ADD 2026
                    If asptSec.Remission IsNot Nothing Then .AddWithValue("AssessmentPlanTreatment_Remission", VerifyBooleanNull(asptSec.Remission))

                End With

                dbo.ExecuteCommand(cmd)

                If asptSec IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "DM", asptSec.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If


            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID:" & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())

        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveCancerDiagnosis(ByVal claimKey As Long, ByVal cancerOnlyDx As List(Of CancerDiagnosisSection),
                                    ByVal dbo As SqlDataObject, ByVal na As Boolean)

        Dim cmd As New SqlClient.SqlCommand

        'Delete all Dx
        cmd = New SqlClient.SqlCommand
        cmd.CommandType = CommandType.Text

        cmd.CommandText = "DELETE FROM Claims_DX WHERE biClaimID=@biClaimID; UPDATE Claims_AHADetail SET CancerDiagnosisNA = @NA WHERE biClaimID = @biClaimID"

        cmd.Parameters.AddWithValue("@biClaimID", claimKey)
        cmd.Parameters.AddWithValue("@NA", na)

        dbo.ExecuteCommand(cmd)

        Try
            If cancerOnlyDx IsNot Nothing Then


                Dim i As Integer = 0

                For Each dx In cancerOnlyDx
                    If Not IsNothing(dx) Then
                        i += 1

                        cmd = New SqlClient.SqlCommand
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.CommandText = "uspClaimsDX_Save_New"

                        With cmd.Parameters
                            .AddWithValue("@biClaimID", claimKey)
                            .AddWithValue("@nIndex", i)
                            .AddWithValue("@sDx", "999.99")
                            .AddWithValue("@bIsNew", 0)
                            .AddWithValue("@bIsDummy", 1)
                            .AddWithValue("@bIsRejected", 0)
                            .AddWithValue("@bIsDeleted", 0)
                            .AddWithValue("@sDxText", VerifyStringNull(dx.Diagnoses))
                            .AddWithValue("@sDxReason", VerifyStringNull(dx.Treatment))
                            .AddWithValue("@Remission", VerifyBooleanNull(dx.Remission))
                            .AddWithValue("@Active", VerifyBooleanNull(dx.Active))
                            .AddWithValue("@Controlled", System.DBNull.Value)
                            .AddWithValue("@History", VerifyBooleanNull(dx.History))
                            .AddWithValue("@Primary", VerifyBooleanNull(dx.Primary))
                            .AddWithValue("@Secondary", VerifyBooleanNull(dx.Secondary))
                            .AddWithValue("@CurrentlyInChemotherapy", VerifyBooleanNull(dx.CurrentlyInChemotherapy))
                            .AddWithValue("@CurrentlyInRadiotherapy", VerifyBooleanNull(dx.CurrentlyInRadiotherapy))
                            .AddWithValue("@CurrentlyInImmunotherapy", VerifyBooleanNull(dx.CurrentlyInImmunotherapy))
                            .AddWithValue("@CurrentlyRefusesTreatment", VerifyBooleanNull(dx.CurrentlyRefusesTreatment))
                        End With

                        dbo.ExecuteCommand(cmd)


                    End If

                Next

            End If

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub



    Private Sub SaveOtherCondition(ByVal claimKey As Long, ByVal otherConditionDx As List(Of OtherCurrentConditionsSection),
                                   ByVal dbo As SqlDataObject, ByVal serviceDate As Date)

        Dim cmd As New SqlClient.SqlCommand
        Dim isDummy As Boolean = True
        Try

            If otherConditionDx IsNot Nothing AndAlso otherConditionDx.Count > 0 Then



                Dim OtherConditionsDT As New DataTable()

                OtherConditionsDT.Columns.Add("ClaimID", GetType(Long))



                OtherConditionsDT.Columns.Add("DxCode", GetType(String))
                OtherConditionsDT.Columns.Add("isDummy", GetType(Boolean))
                OtherConditionsDT.Columns.Add("sDxText", GetType(String))
                OtherConditionsDT.Columns.Add("sDxReason", GetType(String))
                OtherConditionsDT.Columns.Add("Controlled", GetType(Boolean))
                OtherConditionsDT.Columns.Add("ICDCodeType", GetType(Integer))
                OtherConditionsDT.Columns.Add("RejectCode", GetType(String))
                OtherConditionsDT.Columns.Add("RejectedNotes", GetType(String))




                For Each item As OtherCurrentConditionsSection In otherConditionDx
                    Dim row As DataRow = OtherConditionsDT.NewRow()

                    row("DxCode") = IIf(IsNothing(item.DiagnosesCode.Value), "", item.DiagnosesCode.Value)
                    row("isDummy") = (item.DiagnosesCode.Value = "")
                    row("sDxText") = item.Diagnoses.Value
                    row("sDxReason") = item.Treatment.Value

                    If item.Controlled Is Nothing Then
                        row("Controlled") = 0
                    Else
                        row("Controlled") = IIf(IsNothing(item.Controlled.Value), 0, item.Controlled.Value)
                    End If


                    If item.DiagnosesCode Is Nothing OrElse (item.DiagnosesCode.Value Is Nothing OrElse item.DiagnosesCode.Value = "") Then
                        row("ICDCodeType") = 10
                    Else

                        row("ICDCodeType") = AppShared.GetICDCodeType(item.DiagnosesCode.Value, serviceDate)
                    End If


                    row("RejectCode") = AppShared.GetDefaultRejectCode()
                    row("RejectedNotes") = AppShared.GetRejectCodeDescription()
                    OtherConditionsDT.Rows.Add(row)
                Next



                cmd = New SqlClient.SqlCommand
                cmd.CommandTimeout = 10 * 60
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspClaimsDX_Save_New_V2024"


                With cmd.Parameters
                    .AddWithValue("@biClaimID", claimKey)
                    Dim param As New SqlParameter
                    param.ParameterName = "dxTable"
                    param.SqlDbType = SqlDbType.Structured
                    param.Value = OtherConditionsDT
                    .Add(param)
                End With


                If dbo.Connection.State = ConnectionState.Closed Then dbo.OpenConnection()
                dbo.ExecuteCommand(cmd)









                'Try
                '    

                '        Dim i As Integer = 0

                '        For Each dx In otherConditionDx

                '            'If (dx.IsInternal Is Nothing OrElse Not dx.IsInternal.Value.HasValue OrElse Not dx.IsInternal.Value) Then
                '            '    dx.IsInternal = New BooleanField
                '            '    dx.IsInternal.Value = False
                '            'End If

                '            isDummy = True

                '            i += 1

                '            cmd = New SqlClient.SqlCommand
                '            cmd.CommandTimeout = 5 * 60
                '            cmd.CommandType = CommandType.StoredProcedure
                '            cmd.CommandText = "uspClaimsDX_Save_New"

                '            If dx.DiagnosesCode Is Nothing OrElse dx.DiagnosesCode.Value Is Nothing OrElse dx.DiagnosesCode.Value.Trim.Length = 0 Then
                '                If dx.DiagnosesCode Is Nothing Then
                '                    dx.DiagnosesCode = New StringField()
                '                End If
                '                dx.DiagnosesCode.Value = "999.99"
                '            Else
                '                If dx.DiagnosesCode.Value.Trim.Length > 3 AndAlso Not dx.DiagnosesCode.Value.Contains(".") Then
                '                    dx.DiagnosesCode.Value = dx.DiagnosesCode.Value.Insert(3, ".")
                '                End If
                '                isDummy = False
                '            End If

                '            With cmd.Parameters
                '                .AddWithValue("@biClaimID", claimKey)
                '                .AddWithValue("@nIndex", i)
                '                .AddWithValue("@sDx", dx.DiagnosesCode.Value) ' "999.99")
                '                .AddWithValue("@bIsNew", 0)
                '                .AddWithValue("@bIsDummy", isDummy) '1)
                '                .AddWithValue("@bIsRejected", 0)
                '                .AddWithValue("@bIsDeleted", 0)
                '                .AddWithValue("@sDxText", VerifyStringNull(dx.Diagnoses))
                '                .AddWithValue("@sDxReason", VerifyStringNull(dx.Treatment))
                '                .AddWithValue("@Remission", System.DBNull.Value)
                '                .AddWithValue("@Active", System.DBNull.Value)
                '                '.AddWithValue("@IsInternal", IIf(dx.IsInternal.Value, 1, 0))

                '                If (dx.Controlled IsNot Nothing) Then
                '                    .AddWithValue("@Controlled", dx.Controlled.Value)
                '                Else
                '                    .AddWithValue("@Controlled", False)
                '                End If

                '                'Visual Basic evaluates the whole IIf expression, it will throw an error if dx.Controlled is Nothing...
                '                '.AddWithValue("@Controlled", IIf(Not IsNothing(dx.Controlled), dx.Controlled.Value, System.DBNull.Value))

                '                If isDummy = False Then
                '                    .AddWithValue("@nICDCodeType", AppShared.GetICDCodeType(dx.DiagnosesCode.Value, serviceDate))
                '                    .AddWithValue("@IsAHATemplate", True)
                '                    .AddWithValue("@sRejectCode", AppShared.GetDefaultRejectCode)
                '                    .AddWithValue("@sRejectedNotes", AppShared.GetRejectCodeDescription())
                '                End If

                '            End With

                '            If dbo.Connection.State = ConnectionState.Closed Then dbo.OpenConnection()
                '            dbo.ExecuteCommand(cmd)

                '            If dbo.Exception IsNot Nothing Then

                '            End If

                '        Next

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveCKD(ByVal claimKey As Long, ByVal ckdSec As ChronicKidneyDiseaseSection,
                        ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If ckdSec IsNot Nothing Then
                'If Not IsNothing(ckdSec.NA) Then
                '    If Not IsNothing(ckdSec.NA.Value) AndAlso ckdSec.NA.Value Then
                '        ckdSec = New ChronicKidneyDiseaseSection()
                '    End If
                'End If

                'ckdSec = Nothing
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveCKD"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("CKD_NA", VerifyBooleanNull(ckdSec.NA))
                    If ckdSec.Stage IsNot Nothing Then .AddWithValue("CKD_Stage", VerifyNull(ckdSec.Stage.Value))
                    .AddWithValue("CKD_DueToDM", VerifyBooleanNull(ckdSec.DueToDM))
                    .AddWithValue("CKD_DueToOtherCondition", VerifyStringNull(ckdSec.DueToOtherCondition))
                    .AddWithValue("CKD_Controlled", VerifyBooleanNull(ckdSec.Controlled))
                    .AddWithValue("CKD_LowFatDiet", VerifyBooleanNull(ckdSec.LowFatDiet))
                    .AddWithValue("CKD_Dialysis", VerifyBooleanNull(ckdSec.Dialysis))
                    .AddWithValue("CKD_NoMeetDialysis", VerifyBooleanNull(ckdSec.NoMeetDialysis))
                    .AddWithValue("CKD_AdditionalTreatment", VerifyStringNull(ckdSec.AdditionalTreatment))
                    .AddWithValue("Hyperparathyroidism", VerifyBooleanNull(ckdSec.Hyperparathyroidism))
                    .AddWithValue("HyperparathyroidismTreatment", VerifyStringNull(ckdSec.HyperparathyroidismTreatment))
                    .AddWithValue("GFR", ckdSec.GFR)
                    .AddWithValue("SerumCalcium", ckdSec.SerumCalcium)
                    .AddWithValue("SerumPTH", ckdSec.SerumPTH)
                    .AddWithValue("Nephropathy", VerifyBooleanNull(ckdSec.Nephropathy))
                    .AddWithValue("NephropathyType", VerifyStringNull(ckdSec.NephropathyType))
                    .AddWithValue("Nephritis", VerifyBooleanNull(ckdSec.Nephritis))
                    .AddWithValue("NephritisType", VerifyStringNull(ckdSec.NephritisType))
                    .AddWithValue("HasFistula", VerifyBooleanNull(ckdSec.HasFistula))
                    .AddWithValue("CKDBox", VerifyBooleanNull(ckdSec.CKDBox))
                    .AddWithValue("CKD_StressIncontinence", VerifyBooleanNull(ckdSec.StressIncontinence))
                    .AddWithValue("CKD_UrgeIncontinence", VerifyBooleanNull(ckdSec.UrgeIncontinence))
                    .AddWithValue("CKD_PostMicturitionDribble", VerifyBooleanNull(ckdSec.PostMicturitionDribble))
                    .AddWithValue("CKD_OveractiveBladder", VerifyBooleanNull(ckdSec.OveractiveBladder))
                    .AddWithValue("CKD_BladderTreatmentPlan", VerifyStringNull(ckdSec.BladderTreatmentPlan))

                    'ADD 2026 Changes'
                    .AddWithValue("CKD_KidneyTransplant", VerifyBooleanNull(ckdSec.KidneyTransplant))
                    .AddWithValue("CKD_GFRDate", VerifyDateNull(ckdSec.GFRDate))
                    .AddWithValue("CKD_GFROrdered", VerifyBooleanNull(ckdSec.GFROrdered))
                End With

                dbo.ExecuteCommand(cmd)

                If ckdSec IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "ckd", ckdSec.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If

            End If

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SavePressureSore(ByVal claimKey As Long, ByVal pressureSoresSec As PressureSoresSection,
                                 ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If pressureSoresSec IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSavePressureSores"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    If pressureSoresSec.NA IsNot Nothing Then .AddWithValue("PressureSores_NA", VerifyBooleanNull(pressureSoresSec.NA))
                    If pressureSoresSec.HighBackPressureUlcerStage IsNot Nothing Then .AddWithValue("PressureSores_HighBackPressureUlcerStage", VerifyNull(pressureSoresSec.HighBackPressureUlcerStage.Value))
                    If pressureSoresSec.LowBackPressureUlcerStage IsNot Nothing Then .AddWithValue("PressureSores_LowBackPressureUlcerStage", VerifyNull(pressureSoresSec.LowBackPressureUlcerStage.Value))
                    If pressureSoresSec.HipPressureUlcerStageLeft IsNot Nothing Then .AddWithValue("PressureSores_HipPressureUlcerStageLeft", VerifyNull(pressureSoresSec.HipPressureUlcerStageLeft.Value))
                    If pressureSoresSec.HipPressureUlcerStageRight IsNot Nothing Then .AddWithValue("PressureSores_HipPressureUlcerStageRight", VerifyNull(pressureSoresSec.HipPressureUlcerStageRight.Value))
                    If pressureSoresSec.HipPressureUlcerLeft IsNot Nothing Then .AddWithValue("PressureSores_HipPressureUlcerLeft", VerifyBooleanNull(pressureSoresSec.HipPressureUlcerLeft))
                    If pressureSoresSec.HipPressureUlcerRight IsNot Nothing Then .AddWithValue("PressureSores_HipPressureUlcerRight", VerifyBooleanNull(pressureSoresSec.HipPressureUlcerRight))
                    If pressureSoresSec.HeelPressureUlcerStageLeft IsNot Nothing Then .AddWithValue("PressureSores_HeelPressureUlcerStageLeft", VerifyNull(pressureSoresSec.HeelPressureUlcerStageLeft.Value))
                    If pressureSoresSec.HeelPressureUlcerStageRight IsNot Nothing Then .AddWithValue("PressureSores_HeelPressureUlcerStageRight", VerifyNull(pressureSoresSec.HeelPressureUlcerStageRight.Value))
                    If pressureSoresSec.HeelPressureUlcerLeft IsNot Nothing Then .AddWithValue("PressureSores_HeelPressureUlcerLeft", VerifyBooleanNull(pressureSoresSec.HeelPressureUlcerLeft))
                    If pressureSoresSec.HeelPressureUlcerRight IsNot Nothing Then .AddWithValue("PressureSores_HeelPressureUlcerRight", VerifyBooleanNull(pressureSoresSec.HeelPressureUlcerRight))
                    If pressureSoresSec.OtherAreasStage IsNot Nothing Then .AddWithValue("PressureSores_OtherAreasStage", VerifyNull(pressureSoresSec.OtherAreasStage.Value))
                    If pressureSoresSec.OtherAreas IsNot Nothing Then .AddWithValue("PressureSores_OtherAreas", VerifyStringNull(pressureSoresSec.OtherAreas))
                    If pressureSoresSec.Healing IsNot Nothing Then .AddWithValue("PressureSores_Healing", VerifyBooleanNull(pressureSoresSec.Healing))
                    If pressureSoresSec.Healed IsNot Nothing Then .AddWithValue("PressureSores_Healed", VerifyBooleanNull(pressureSoresSec.Healed))
                    If pressureSoresSec.Worse IsNot Nothing Then .AddWithValue("PressureSores_Worse", VerifyBooleanNull(pressureSoresSec.Worse))
                    If pressureSoresSec.Hydrocolloid IsNot Nothing Then .AddWithValue("PressureSores_Hydrocolloid", VerifyBooleanNull(pressureSoresSec.Hydrocolloid))
                    If pressureSoresSec.SilverDressing IsNot Nothing Then .AddWithValue("PressureSores_SilverDressing", VerifyBooleanNull(pressureSoresSec.SilverDressing))
                    If pressureSoresSec.Hydrogel IsNot Nothing Then .AddWithValue("PressureSores_Hydrogel", VerifyBooleanNull(pressureSoresSec.Hydrogel))
                    If pressureSoresSec.Antibiotic IsNot Nothing Then .AddWithValue("PressureSores_Antibiotic", VerifyBooleanNull(pressureSoresSec.Antibiotic))
                    If pressureSoresSec.Alginate IsNot Nothing Then .AddWithValue("PressureSores_Alginate", VerifyBooleanNull(pressureSoresSec.Alginate))
                    If pressureSoresSec.Enzyme IsNot Nothing Then .AddWithValue("PressureSores_Enzyme", VerifyBooleanNull(pressureSoresSec.Enzyme))
                    If pressureSoresSec.TransparentDressing IsNot Nothing Then .AddWithValue("PressureSores_TransparentDressing", VerifyBooleanNull(pressureSoresSec.TransparentDressing))
                    If pressureSoresSec.OthersTreatment IsNot Nothing Then .AddWithValue("PressureSores_OthersTreatment", VerifyStringNull(pressureSoresSec.OthersTreatment))
                    If pressureSoresSec.ByPressure IsNot Nothing Then .AddWithValue("PressureSores_ByPressure", VerifyBooleanNull(pressureSoresSec.ByPressure))
                    If pressureSoresSec.Chronicle IsNot Nothing Then .AddWithValue("PressureSores_Chronicle", VerifyBooleanNull(pressureSoresSec.Chronicle))
                    If pressureSoresSec.AnatomicalSite IsNot Nothing Then .AddWithValue("PressureSores_AnatomicalSite", VerifyStringNull(pressureSoresSec.AnatomicalSite))
                End With


                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveOtherConditionAdditional(ByVal claimKey As Long, ByVal otherConditionAdditional As OtherCurrentConditionsAdditionalSection,
                                             ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            If otherConditionAdditional IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveOtherConditionAdditional"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("OtherCurrentConditionAdditional", VerifyStringNull(otherConditionAdditional.AdditionalRecomendation))
                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SavePressureSoreList(ByVal claimKey As Long, ByVal pressureSoresSecList As List(Of PressureSoresSection),
                                 ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try
            Dim i As Integer = 0

            'Delete all Pressure Sores
            cmd = New SqlClient.SqlCommand

            cmd.CommandType = CommandType.Text
            cmd.CommandText = "DELETE FROM Claims_PressureSores WHERE biClaimID = @biClaimID"
            cmd.Parameters.AddWithValue("@biClaimID", claimKey)

            dbo.ExecuteCommand(cmd)

            If Not IsNothing(pressureSoresSecList) Then
                For Each ps In pressureSoresSecList

                    i += 1

                    cmd = New SqlClient.SqlCommand

                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "uspClaims_PressureSores_Save"

                    With cmd.Parameters
                        .AddWithValue("@biClaimID", claimKey)
                        .AddWithValue("@nIndex", i)
                        .AddWithValue("@ByVaricoseVainsInLegs", IIf(Not IsNothing(ps.ByVaricoseVainsInLegs), ps.ByVaricoseVainsInLegs.Value, False))
                        .AddWithValue("@ByArteriosclerosisInExtremities", IIf(Not IsNothing(ps.ByArteriosclerosisInExtremities), ps.ByArteriosclerosisInExtremities.Value, False))
                        .AddWithValue("@ByDiabetic", IIf(Not IsNothing(ps.ByDiabetic), ps.ByDiabetic.Value, False))
                        .AddWithValue("@ByPressure", IIf(Not IsNothing(ps.ByPressure), ps.ByPressure.Value, False))
                        .AddWithValue("@ByPressureStage", IIf(Not IsNothing(ps.ByPressureStage), ps.ByPressureStage.Value, -1))
                        .AddWithValue("@AnatomicalSite", IIf(Not IsNothing(ps.AnatomicalSite), ps.AnatomicalSite.Value, ""))
                        .AddWithValue("@AnatomicalSiteOther", IIf(Not IsNothing(ps.AnatomicalSiteOther), ps.AnatomicalSiteOther.Value, ""))
                        .AddWithValue("@ByOtherCondition", IIf(Not IsNothing(ps.ByOtherCondition), ps.ByOtherCondition.Value, False))
                        .AddWithValue("@OtherConditionText", IIf(Not IsNothing(ps.OtherConditionText), ps.OtherConditionText.Value, ""))
                        .AddWithValue("@Treatment", IIf(Not IsNothing(ps.Treatment), ps.Treatment.Value, ""))
                    End With

                    dbo.ExecuteCommand(cmd)

                Next
            End If


        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveDiseasesOfTheSkin(ByVal claimKey As Long, ByVal diseasesOfTheSkinList As List(Of DiseasesOfTheSkin),
                                      ByVal dbo As SqlDataObject, ByVal na As Boolean)
        Dim cmd As New SqlClient.SqlCommand
        Dim isDummy As Boolean = True


        Try
            Dim i As Integer = 0
            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = "uspSaveDiseasesOfTheSkin2022"
            Dim dt As New DataTable()

            dt.Columns.Add("biClaimID", Type.GetType("System.Int64"))
            dt.Columns.Add("IndexRow", Type.GetType("System.Int16"))
            dt.Columns.Add("Dermatitis", Type.GetType("System.Boolean"))
            dt.Columns.Add("DermatitisTreatment", Type.GetType("System.String"))
            dt.Columns.Add("DermatitisTypeLocation", Type.GetType("System.String"))
            dt.Columns.Add("Psoriasis", Type.GetType("System.Boolean"))
            dt.Columns.Add("PsoriasisType", Type.GetType("System.String"))
            dt.Columns.Add("PsoriasicArthritis", Type.GetType("System.Boolean"))
            dt.Columns.Add("PsoriasicArthritisLocation", Type.GetType("System.String"))
            dt.Columns.Add("PsoriasisTreatment", Type.GetType("System.String"))
            dt.Columns.Add("Ulcer", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerLocationAndDepth", Type.GetType("System.String"))
            dt.Columns.Add("DueToArteriosclerosisInExtremities", Type.GetType("System.Boolean"))
            dt.Columns.Add("DueToPVD", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerTreatment", Type.GetType("System.String"))
            dt.Columns.Add("PressureUlcer", Type.GetType("System.Boolean"))
            dt.Columns.Add("PressureUlcerStage", Type.GetType("System.Int16"))
            dt.Columns.Add("PressureUlcerNoStage", Type.GetType("System.Boolean"))
            dt.Columns.Add("PressureUlcerLocation", Type.GetType("System.String"))
            dt.Columns.Add("PressureUlcerTreatment", Type.GetType("System.String"))
            dt.Columns.Add("PressureUlcerOtherCause", Type.GetType("System.Boolean"))
            dt.Columns.Add("PressureUlcerOtherCauseText", Type.GetType("System.String"))
            dt.Columns.Add("PressureUlcerOtherCauseTreatment", Type.GetType("System.String"))
            dt.Columns.Add("UlcerDepth", Type.GetType("System.String"))
            dt.Columns.Add("UlcerLocation", Type.GetType("System.String"))
            dt.Columns.Add("UlcerDueToDiabetes", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerDueToVaricoseVeins", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerDueToVaricoseVeinsWithInflamation", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerDueToIdiopathicVenousHypertension", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerDueToIdiopathicVenousHypertensionWithInflamation", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerDueToOtherCause", Type.GetType("System.Boolean"))
            dt.Columns.Add("UlcerDueToOtherCauseText", Type.GetType("System.String"))
            'dt.Columns.Add("UlcerDueToPVDWithInflamation", Type.GetType("System.Boolean"))

            cmd.Parameters.AddWithValue("NA", na)

            If Not IsNothing(diseasesOfTheSkinList) Then

                For Each ds In diseasesOfTheSkinList
                    i += 1
                    Dim myRow As DataRow
                    myRow = dt.NewRow()
                    myRow("biClaimID") = claimKey
                    myRow("IndexRow") = i
                    myRow("Dermatitis") = VerifyBooleanNull(ds.Dermatitis)
                    myRow("DermatitisTreatment") = VerifyStringNull(ds.DermatitisTreatment)
                    myRow("DermatitisTypeLocation") = VerifyStringNull(ds.DermatitisTypeLocation)
                    myRow("Psoriasis") = VerifyBooleanNull(ds.Psoriasis)
                    myRow("PsoriasisType") = VerifyStringNull(ds.PsoriasisType)
                    myRow("PsoriasicArthritis") = VerifyBooleanNull(ds.PsoriasicArthritis)
                    myRow("PsoriasicArthritisLocation") = VerifyStringNull(ds.PsoriasicArthritisLocation)
                    myRow("PsoriasisTreatment") = VerifyStringNull(ds.PsoriasisTreatment)
                    myRow("Ulcer") = VerifyBooleanNull(ds.Ulcer)
                    myRow("UlcerLocationAndDepth") = VerifyStringNull(ds.UlcerLocationAndDepth)
                    myRow("DueToArteriosclerosisInExtremities") = VerifyBooleanNull(ds.DueToArteriosclerosisInExtremities)
                    myRow("DueToPVD") = VerifyBooleanNull(ds.DueToPVD)
                    myRow("UlcerTreatment") = VerifyStringNull(ds.UlcerTreatment)
                    myRow("PressureUlcer") = VerifyBooleanNull(ds.PressureUlcer)

                    If IsNothing(ds.PressureUlcerStage) Then
                        ds.PressureUlcerStage = New IntegerField()
                    End If

                    myRow("PressureUlcerStage") = VerifyIntegerNull(ds.PressureUlcerStage)
                    myRow("PressureUlcerNoStage") = VerifyBooleanNull(ds.PressureUlcerNoStage)
                    myRow("PressureUlcerLocation") = VerifyStringNull(ds.PressureUlcerLocation)
                    myRow("PressureUlcerTreatment") = VerifyStringNull(ds.PressureUlcerTreatment)

                    myRow("PressureUlcerOtherCause") = VerifyBooleanNull(ds.PressureUlcerOtherCause)
                    myRow("PressureUlcerOtherCauseText") = VerifyStringNull(ds.PressureUlcerOtherCauseText)
                    myRow("PressureUlcerOtherCauseTreatment") = VerifyStringNull(ds.PressureUlcerOtherCauseTreatment)

                    myRow("UlcerDepth") = VerifyStringNull(ds.UlcerDepth)
                    myRow("UlcerLocation") = VerifyStringNull(ds.UlcerLocation)

                    myRow("UlcerDueToDiabetes") = VerifyBooleanNull(ds.UlcerDueToDiabetes)
                    myRow("UlcerDueToVaricoseVeins") = VerifyBooleanNull(ds.UlcerDueToVaricoseVeins)
                    myRow("UlcerDueToVaricoseVeinsWithInflamation") = VerifyBooleanNull(ds.UlcerDueToVaricoseVeinsWithInflamation)
                    myRow("UlcerDueToIdiopathicVenousHypertension") = VerifyBooleanNull(ds.UlcerDueToIdiopathicVenousHypertension)
                    myRow("UlcerDueToIdiopathicVenousHypertensionWithInflamation") = VerifyBooleanNull(ds.UlcerDueToIdiopathicVenousHypertensionWithInflamation)
                    myRow("UlcerDueToOtherCause") = VerifyBooleanNull(ds.UlcerDueToOtherCause)
                    myRow("UlcerDueToOtherCauseText") = VerifyStringNull(ds.UlcerDueToOtherCauseText)
                    'myRow("UlcerDueToPVDWithInflamation") = VerifyBooleanNull(ds.UlcerDueToPVDWithInflamation)

                    dt.Rows.Add(myRow)


                    If dt IsNot Nothing AndAlso ds.MedicationList IsNot Nothing Then
                        'Dim s As Boolean = SaveMedicationbySection(claimKey, "SD", ds.MedicationList, dbo)
                        'manage if error ocurred exaption is been handled
                    End If
                Next

                Dim param As New SqlParameter("DiseasesSkinTable", SqlDbType.Structured)
                param.Value = dt

                cmd.Parameters.AddWithValue("ClaimID", claimKey)
                cmd.Parameters.Add(param)

                dbo.ExecuteCommand(cmd)


            Else
                Dim myRow As DataRow
                Dim ds As New DiseasesOfTheSkin()

                myRow = dt.NewRow()
                myRow("biClaimID") = claimKey
                myRow("IndexRow") = 0
                myRow("Dermatitis") = VerifyBooleanNull(ds.Dermatitis)
                myRow("DermatitisTreatment") = VerifyStringNull(ds.DermatitisTreatment)
                myRow("DermatitisTypeLocation") = VerifyStringNull(ds.DermatitisTypeLocation)
                myRow("Psoriasis") = VerifyBooleanNull(ds.Psoriasis)
                myRow("PsoriasisType") = VerifyStringNull(ds.PsoriasisType)
                myRow("PsoriasicArthritis") = VerifyBooleanNull(ds.PsoriasicArthritis)
                myRow("PsoriasicArthritisLocation") = VerifyStringNull(ds.PsoriasicArthritisLocation)
                myRow("PsoriasisTreatment") = VerifyStringNull(ds.PsoriasisTreatment)
                myRow("Ulcer") = VerifyBooleanNull(ds.Ulcer)
                myRow("UlcerLocationAndDepth") = VerifyStringNull(ds.UlcerLocationAndDepth)
                myRow("DueToArteriosclerosisInExtremities") = VerifyBooleanNull(ds.DueToArteriosclerosisInExtremities)
                myRow("DueToPVD") = VerifyBooleanNull(ds.DueToPVD)
                myRow("UlcerTreatment") = VerifyStringNull(ds.UlcerTreatment)
                myRow("PressureUlcer") = VerifyBooleanNull(ds.PressureUlcer)

                If IsNothing(ds.PressureUlcerStage) Then
                    ds.PressureUlcerStage = New IntegerField()

                End If

                myRow("PressureUlcerStage") = VerifyIntegerNull(ds.PressureUlcerStage)
                myRow("PressureUlcerNoStage") = VerifyBooleanNull(ds.PressureUlcerNoStage)
                myRow("PressureUlcerLocation") = VerifyStringNull(ds.PressureUlcerLocation)
                myRow("PressureUlcerTreatment") = VerifyStringNull(ds.PressureUlcerTreatment)

                myRow("PressureUlcerOtherCause") = VerifyBooleanNull(ds.PressureUlcerOtherCause)
                myRow("PressureUlcerOtherCauseText") = VerifyStringNull(ds.PressureUlcerOtherCauseText)
                myRow("PressureUlcerOtherCauseTreatment") = VerifyStringNull(ds.PressureUlcerOtherCauseTreatment)

                myRow("UlcerDepth") = VerifyStringNull(ds.UlcerDepth)
                myRow("UlcerLocation") = VerifyStringNull(ds.UlcerLocation)

                myRow("UlcerDueToDiabetes") = VerifyBooleanNull(ds.UlcerDueToDiabetes)
                myRow("UlcerDueToVaricoseVeins") = VerifyBooleanNull(ds.UlcerDueToVaricoseVeins)
                myRow("UlcerDueToVaricoseVeinsWithInflamation") = VerifyBooleanNull(ds.UlcerDueToVaricoseVeinsWithInflamation)
                myRow("UlcerDueToIdiopathicVenousHypertension") = VerifyBooleanNull(ds.UlcerDueToIdiopathicVenousHypertension)
                myRow("UlcerDueToIdiopathicVenousHypertensionWithInflamation") = VerifyBooleanNull(ds.UlcerDueToIdiopathicVenousHypertensionWithInflamation)
                myRow("UlcerDueToOtherCause") = VerifyBooleanNull(ds.UlcerDueToOtherCause)
                myRow("UlcerDueToOtherCauseText") = VerifyStringNull(ds.UlcerDueToOtherCauseText)
                'myRow("UlcerDueToPVDWithInflamation") = VerifyBooleanNull(ds.UlcerDueToPVDWithInflamation)

                dt.Rows.Add(myRow)
                Dim param As New SqlParameter("DiseasesSkinTable", SqlDbType.Structured)
                param.Value = dt

                cmd.Parameters.AddWithValue("ClaimID", claimKey)
                cmd.Parameters.Add(param)

                dbo.ExecuteCommand(cmd)
            End If

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally

        End Try

    End Sub

    Private Sub SaveMajorDepression(ByVal claimKey As Long, ByVal majorDepressionSec As MajorDepressionSection,
                                    ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try

            If IsNothing(majorDepressionSec) Then
                majorDepressionSec = New MajorDepressionSection()
            End If

            If majorDepressionSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMayorDepression"
                'majorDepressionSec.MentalHealth_ScreeningSubstanceUseDatePerformed.Value = Nothing
                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)

                    .AddWithValue("NA", majorDepressionSec.NA)

                    If majorDepressionSec.IsMajorDepression IsNot Nothing Then .AddWithValue("IsMajorDepression", VerifyBooleanNull(majorDepressionSec.IsMajorDepression))
                    If majorDepressionSec.InRemission IsNot Nothing Then .AddWithValue("InRemission", VerifyBooleanNull(majorDepressionSec.InRemission))
                    If majorDepressionSec.Recurrent IsNot Nothing Then .AddWithValue("Recurrent", VerifyBooleanNull(majorDepressionSec.Recurrent))
                    If majorDepressionSec.MildSeverity IsNot Nothing Then .AddWithValue("MildSeverity", VerifyBooleanNull(majorDepressionSec.MildSeverity))
                    If majorDepressionSec.ModerateSeverity IsNot Nothing Then .AddWithValue("ModerateSeverity", VerifyBooleanNull(majorDepressionSec.ModerateSeverity))
                    If majorDepressionSec.SevereSeverity IsNot Nothing Then .AddWithValue("SevereSeverity", VerifyBooleanNull(majorDepressionSec.SevereSeverity))
                    If majorDepressionSec.MentalHealth_SeverWithoutPsychoticSymptoms IsNot Nothing Then .AddWithValue("MentalHealth_SeverWithoutPsychoticSymptoms", VerifyBooleanNull(majorDepressionSec.MentalHealth_SeverWithoutPsychoticSymptoms))
                    If majorDepressionSec.TreatmentPlan IsNot Nothing Then .AddWithValue("TreatmentPlan", VerifyStringNull(majorDepressionSec.TreatmentPlan))

                    If majorDepressionSec.SingleEpisode IsNot Nothing Then .AddWithValue("SingleEpisode", VerifyBooleanNull(majorDepressionSec.SingleEpisode))
                    If majorDepressionSec.PsychoticSymptoms IsNot Nothing Then .AddWithValue("PsychoticSymptoms", VerifyBooleanNull(majorDepressionSec.PsychoticSymptoms))
                    If majorDepressionSec.BipolarDisorder IsNot Nothing Then .AddWithValue("BipolarDisorder", VerifyBooleanNull(majorDepressionSec.BipolarDisorder))
                    If majorDepressionSec.BipolarDisorderTypeAndSeverity IsNot Nothing Then .AddWithValue("BipolarDisorderTypeAndSeverity", VerifyStringNull(majorDepressionSec.BipolarDisorderTypeAndSeverity))
                    If majorDepressionSec.BipolarDisorderTreatmentPlan IsNot Nothing Then .AddWithValue("BipolarDisorderTreatmentPlan", VerifyStringNull(majorDepressionSec.BipolarDisorderTreatmentPlan))
                    If majorDepressionSec.SchizophreniaType IsNot Nothing Then .AddWithValue("SchizophreniaType", VerifyStringNull(majorDepressionSec.SchizophreniaType))
                    If majorDepressionSec.Schizophrenia IsNot Nothing Then .AddWithValue("Schizophrenia", VerifyBooleanNull(majorDepressionSec.Schizophrenia))
                    If majorDepressionSec.SchizophreniaTreatmentPlan IsNot Nothing Then .AddWithValue("SchizophreniaTreatmentPlan", VerifyStringNull(majorDepressionSec.SchizophreniaTreatmentPlan))
                    If majorDepressionSec.MoodDisorder IsNot Nothing Then .AddWithValue("MoodDisorder", VerifyBooleanNull(majorDepressionSec.MoodDisorder))
                    If majorDepressionSec.MoodDisorderComments IsNot Nothing Then .AddWithValue("MoodDisorderComments", VerifyStringNull(majorDepressionSec.MoodDisorderComments))

                    If majorDepressionSec.PHQ9DoneDate IsNot Nothing AndAlso
                    Globals.ValidateDateMinMaxRange(majorDepressionSec.PHQ9DoneDate.Value) Then
                        .AddWithValue("PHQ9DoneDate", VerifyDateNull(majorDepressionSec.PHQ9DoneDate))
                    End If

                    If majorDepressionSec.PHQ9ScoreResult IsNot Nothing Then .AddWithValue("PHQ9ScoreResult", VerifyStringNull(majorDepressionSec.PHQ9ScoreResult))
                    If majorDepressionSec.Dysthymia IsNot Nothing Then .AddWithValue("Dysthymia", VerifyBooleanNull(majorDepressionSec.Dysthymia))
                    If majorDepressionSec.DysthymiaComments IsNot Nothing Then .AddWithValue("DysthymiaComments", VerifyStringNull(majorDepressionSec.DysthymiaComments))
                    If majorDepressionSec.UseOfSubtancesTreatmentPlan IsNot Nothing Then .AddWithValue("UseOfSubtancesTreatment", VerifyStringNull(majorDepressionSec.UseOfSubtancesTreatmentPlan))
                    If majorDepressionSec.PHQ9ReasonNotDoneID IsNot Nothing Then .AddWithValue("PHQ9ReasonNotDoneID", VerifyIntegerNull(majorDepressionSec.PHQ9ReasonNotDoneID))
                    If majorDepressionSec.PHQ9ReasonNotDoneOther IsNot Nothing Then .AddWithValue("PHQ9ReasonNotDoneOther", VerifyStringNull(majorDepressionSec.PHQ9ReasonNotDoneOther))
                    If majorDepressionSec.MentalHealth_SubstanceAbuseFreeText IsNot Nothing Then .AddWithValue("MentalHealth_SubstanceAbuseFreeText", VerifyStringNull(majorDepressionSec.MentalHealth_SubstanceAbuseFreeText))


                    If majorDepressionSec.MentalHealth_ScreeningSubstanceUseDatePerformed IsNot Nothing AndAlso
                    Globals.ValidateDateMinMaxRange(majorDepressionSec.MentalHealth_ScreeningSubstanceUseDatePerformed.Value) Then
                        .AddWithValue("MentalHealth_ScreeningSubstanceUseDatePerformed", VerifyDateNull(majorDepressionSec.MentalHealth_ScreeningSubstanceUseDatePerformed))
                    End If
                    If majorDepressionSec.MentalHealth_SubstanceAbuseCheckBox IsNot Nothing Then .AddWithValue("MentalHealth_SubstanceAbuseCheckBox", VerifyBooleanNull(majorDepressionSec.MentalHealth_SubstanceAbuseCheckBox))


                    'ADD 2026 Changes'
                    If majorDepressionSec.GeneralizedAnxietyDisorder IsNot Nothing Then .AddWithValue("GeneralizedAnxietyDisorder", VerifyBooleanNull(majorDepressionSec.GeneralizedAnxietyDisorder))
                    If majorDepressionSec.OtherAnxiety IsNot Nothing Then .AddWithValue("OtherAnxiety", VerifyBooleanNull(majorDepressionSec.OtherAnxiety))
                    If majorDepressionSec.OtherAnxietyText IsNot Nothing Then .AddWithValue("OtherAnxietyText", VerifyStringNull(majorDepressionSec.OtherAnxietyText))
                    If majorDepressionSec.GeneralizedAnxietyDisorderComments IsNot Nothing Then .AddWithValue("GeneralizedAnxietyDisorderComments", VerifyStringNull(majorDepressionSec.GeneralizedAnxietyDisorderComments))

                    'PHQ9 Fields
                    If majorDepressionSec.PHQ9 IsNot Nothing Then

                        If majorDepressionSec.PHQ9.Q1 IsNot Nothing Then .AddWithValue("PHQ9_Q1", VerifyIntegerNull(majorDepressionSec.PHQ9.Q1))
                        If majorDepressionSec.PHQ9.Q2 IsNot Nothing Then .AddWithValue("PHQ9_Q2", VerifyIntegerNull(majorDepressionSec.PHQ9.Q2))
                        If majorDepressionSec.PHQ9.Q3 IsNot Nothing Then .AddWithValue("PHQ9_Q3", VerifyIntegerNull(majorDepressionSec.PHQ9.Q3))
                        If majorDepressionSec.PHQ9.Q4 IsNot Nothing Then .AddWithValue("PHQ9_Q4", VerifyIntegerNull(majorDepressionSec.PHQ9.Q4))
                        If majorDepressionSec.PHQ9.Q5 IsNot Nothing Then .AddWithValue("PHQ9_Q5", VerifyIntegerNull(majorDepressionSec.PHQ9.Q5))
                        If majorDepressionSec.PHQ9.Q6 IsNot Nothing Then .AddWithValue("PHQ9_Q6", VerifyIntegerNull(majorDepressionSec.PHQ9.Q6))
                        If majorDepressionSec.PHQ9.Q7 IsNot Nothing Then .AddWithValue("PHQ9_Q7", VerifyIntegerNull(majorDepressionSec.PHQ9.Q7))
                        If majorDepressionSec.PHQ9.Q8 IsNot Nothing Then .AddWithValue("PHQ9_Q8", VerifyIntegerNull(majorDepressionSec.PHQ9.Q8))
                        If majorDepressionSec.PHQ9.Q9 IsNot Nothing Then .AddWithValue("PHQ9_Q9", VerifyIntegerNull(majorDepressionSec.PHQ9.Q9))

                        If majorDepressionSec.PHQ9.TotalCol1 IsNot Nothing Then .AddWithValue("PHQ9_TotalCol1", VerifyIntegerNull(majorDepressionSec.PHQ9.TotalCol1))
                        If majorDepressionSec.PHQ9.TotalCol2 IsNot Nothing Then .AddWithValue("PHQ9_TotalCol2", VerifyIntegerNull(majorDepressionSec.PHQ9.TotalCol2))
                        If majorDepressionSec.PHQ9.TotalCol3 IsNot Nothing Then .AddWithValue("PHQ9_TotalCol3", VerifyIntegerNull(majorDepressionSec.PHQ9.TotalCol3))
                        If majorDepressionSec.PHQ9.NotDifficultAtAll IsNot Nothing Then .AddWithValue("PHQ9_NotDifficultAtAll", VerifyBooleanNull(majorDepressionSec.PHQ9.NotDifficultAtAll))
                        If majorDepressionSec.PHQ9.SomewhatDifficult IsNot Nothing Then .AddWithValue("PHQ9_SomewhatDifficult", VerifyBooleanNull(majorDepressionSec.PHQ9.SomewhatDifficult))
                        If majorDepressionSec.PHQ9.VeryDifficult IsNot Nothing Then .AddWithValue("PHQ9_VeryDifficult", VerifyBooleanNull(majorDepressionSec.PHQ9.VeryDifficult))
                        If majorDepressionSec.PHQ9.ExtremelyDifficult IsNot Nothing Then .AddWithValue("PHQ9_ExtremeDifficult", VerifyBooleanNull(majorDepressionSec.PHQ9.ExtremelyDifficult))

                        If majorDepressionSec.PHQ9.DepressedPastYear IsNot Nothing Then .AddWithValue("PHQ9_DepressedPastYear", VerifyBooleanNull(majorDepressionSec.PHQ9.DepressedPastYear))
                        If majorDepressionSec.PHQ9.SuicidePastMonth IsNot Nothing Then .AddWithValue("PHQ9_SuicidePastMonth", VerifyBooleanNull(majorDepressionSec.PHQ9.SuicidePastMonth))
                        If majorDepressionSec.PHQ9.TriedSuicide IsNot Nothing Then .AddWithValue("PHQ9_TriedSuicide", VerifyBooleanNull(majorDepressionSec.PHQ9.TriedSuicide))

                    Else
                        majorDepressionSec.PHQ9 = New PHQ9
                    End If

                    If majorDepressionSec.ADHD IsNot Nothing Then .AddWithValue("ADHD", VerifyBooleanNull(majorDepressionSec.ADHD))
                    If majorDepressionSec.ADHDComments IsNot Nothing Then .AddWithValue("ADHDComments", VerifyStringNull(majorDepressionSec.ADHDComments))

                    If majorDepressionSec.Autism IsNot Nothing Then .AddWithValue("Autism", VerifyBooleanNull(majorDepressionSec.Autism))
                    If majorDepressionSec.AutismComments IsNot Nothing Then .AddWithValue("AutismComments", VerifyStringNull(majorDepressionSec.AutismComments))

                End With

                dbo.ExecuteCommand(cmd)
                If majorDepressionSec IsNot Nothing AndAlso majorDepressionSec.MedicationList IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "eyeAndNeurology", majorDepressionSec.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveCardiovascularDiseases(ByVal claimKey As Long, ByVal cardiovascularDiseases As CardiovascularDiseases, ByVal dbo As SqlDataObject)

        Try

            If cardiovascularDiseases Is Nothing Then cardiovascularDiseases = New CardiovascularDiseases

            'If Not IsNothing(cardiovascularDiseases.NA) Then
            '    If Not IsNothing(cardiovascularDiseases.NA.Value) AndAlso cardiovascularDiseases.NA.Value Then
            '        cardiovascularDiseases = New CardiovascularDiseases()
            '    End If
            'End If

            Using cmd As New SqlCommand

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveCardiovascularDiseases"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("CardiovascularDiseasesNA", VerifyBooleanNull(cardiovascularDiseases.NA))
                    .AddWithValue("ArterialHypertension", VerifyBooleanNull(cardiovascularDiseases.ArterialHypertension))
                    .AddWithValue("PulmonaryHypertension", VerifyBooleanNull(cardiovascularDiseases.PulmonaryHypertension))
                    .AddWithValue("PulmonaryHypertensionType", VerifyStringNull(cardiovascularDiseases.PulmonaryHypertensionType))
                    .AddWithValue("HeartFailure", VerifyBooleanNull(cardiovascularDiseases.HeartFailure))
                    .AddWithValue("Congestive", VerifyBooleanNull(cardiovascularDiseases.Congestive))
                    .AddWithValue("Diastolic", VerifyBooleanNull(cardiovascularDiseases.Diastolic))
                    .AddWithValue("Systolic", VerifyBooleanNull(cardiovascularDiseases.Systolic))
                    .AddWithValue("Chronic", VerifyBooleanNull(cardiovascularDiseases.Chronic))
                    .AddWithValue("PVD", VerifyBooleanNull(cardiovascularDiseases.PVD))
                    .AddWithValue("AtrilaFibrillation", VerifyBooleanNull(cardiovascularDiseases.AtrilaFibrillation))
                    .AddWithValue("AtrilFibrillationType", VerifyStringNull(cardiovascularDiseases.AtrilFibrillationType))
                    .AddWithValue("Arteriosclerosis", VerifyBooleanNull(cardiovascularDiseases.Arteriosclerosis))
                    .AddWithValue("Aorta", VerifyBooleanNull(cardiovascularDiseases.Aorta))
                    .AddWithValue("Crowns", VerifyBooleanNull(cardiovascularDiseases.Crowns))
                    .AddWithValue("RenalArtery", VerifyBooleanNull(cardiovascularDiseases.RenalArtery))
                    .AddWithValue("ArteriosclerosisExtremities", VerifyBooleanNull(cardiovascularDiseases.ArteriosclerosisExtremities))
                    .AddWithValue("LegLT", VerifyBooleanNull(cardiovascularDiseases.LegLT))
                    .AddWithValue("LegRT", VerifyBooleanNull(cardiovascularDiseases.LegRT))
                    .AddWithValue("ArmLT", VerifyBooleanNull(cardiovascularDiseases.ArmLT))
                    .AddWithValue("ArmRT", VerifyBooleanNull(cardiovascularDiseases.ArmRT))
                    .AddWithValue("IntermittentClaudication", VerifyBooleanNull(cardiovascularDiseases.IntermittentClaudication))
                    .AddWithValue("RestPain", VerifyBooleanNull(cardiovascularDiseases.RestPain))
                    .AddWithValue("OtherComplications", VerifyBooleanNull(cardiovascularDiseases.OtherComplications))
                    .AddWithValue("OtherComplicationsText", VerifyStringNull(cardiovascularDiseases.OtherComplicationsText))
                    .AddWithValue("HypertensionTreatmentPlan", VerifyStringNull(cardiovascularDiseases.HypertensionTreatmentPlan))
                    .AddWithValue("PVDTreatmentPlan", VerifyStringNull(cardiovascularDiseases.PVDTreatmentPlan))
                    .AddWithValue("ArteriosclerosisTreatmentPlan", VerifyStringNull(cardiovascularDiseases.ArteriosclerosisTreatmentPlan))
                    .AddWithValue("AnginaPectoris", VerifyBooleanNull(cardiovascularDiseases.AnginaPectoris))
                    .AddWithValue("SSS", VerifyBooleanNull(cardiovascularDiseases.SSS))
                    .AddWithValue("SVT", VerifyBooleanNull(cardiovascularDiseases.SVT))
                    .AddWithValue("Pacemaker", VerifyBooleanNull(cardiovascularDiseases.Pacemaker))
                    .AddWithValue("CAD", VerifyBooleanNull(cardiovascularDiseases.CAD))
                    .AddWithValue("Cardiomiopatia", VerifyBooleanNull(cardiovascularDiseases.Cardiomiopatia))
                    .AddWithValue("MyocardialInfarction", VerifyBooleanNull(cardiovascularDiseases.MyocardialInfarction))
                    .AddWithValue("Cardiomegaly", VerifyBooleanNull(cardiovascularDiseases.Cardiomegaly))
                    .AddWithValue("AtrioventricularBlock", VerifyBooleanNull(cardiovascularDiseases.AtrioventricularBlock))
                    .AddWithValue("AtrioventricularBlockDegree", VerifyStringNull(cardiovascularDiseases.AtrioventricularBlockDegree))
                    .AddWithValue("VaricoseVeinsOfLowerExtremityWithPain", VerifyBooleanNull(cardiovascularDiseases.VaricoseVeinsOfLowerExtremityWithPain))
                    .AddWithValue("ConductionDisorder", VerifyBooleanNull(cardiovascularDiseases.ConductionDisorder))
                    .AddWithValue("MyocardialInfarctionTreatmentPlan", VerifyStringNull(cardiovascularDiseases.MyocardialInfarctionTreatmentPlan))

                    'Add 2026 Changes'

                    .AddWithValue("OldMyocardialInfarction", VerifyBooleanNull(cardiovascularDiseases.OldMyocardialInfarction))
                    .AddWithValue("Hyperlipidemia", VerifyBooleanNull(cardiovascularDiseases.Hyperlipidemia))
                    .AddWithValue("HyperlipidemiaText", VerifyStringNull(cardiovascularDiseases.HyperlipidemiaText))
                    .AddWithValue("HeartTransplant", VerifyBooleanNull(cardiovascularDiseases.HeartTransplant))


                End With

                dbo.ExecuteCommand(cmd)
                If cardiovascularDiseases IsNot Nothing AndAlso cardiovascularDiseases.MedicationList IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "cardio", cardiovascularDiseases.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If
            End Using

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally

        End Try

    End Sub

    Private Sub SaveEyeAndNeurology(ByVal claimKey As Long, ByVal eyeAndNeurology As EyesAndNeurology, ByVal dbo As SqlDataObject)

        Try
            If eyeAndNeurology Is Nothing Then eyeAndNeurology = New EyesAndNeurology()

            'If Not IsNothing(eyeAndNeurology.NA) Then
            '    If Not IsNothing(eyeAndNeurology.NA.Value) AndAlso eyeAndNeurology.NA.Value Then
            '        eyeAndNeurology = New EyesAndNeurology()
            '    End If
            'End If
            Using cmd As New SqlCommand

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveEyeAndNeurology"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("EyesAndNeurologyNA", VerifyBooleanNull(eyeAndNeurology.NA))
                    .AddWithValue("Retinopathy", VerifyBooleanNull(eyeAndNeurology.Retinopathy))
                    .AddWithValue("Proliferative", VerifyBooleanNull(eyeAndNeurology.Proliferative))
                    .AddWithValue("ProliferativeEyeRT", VerifyBooleanNull(eyeAndNeurology.ProliferativeEyeRT))
                    .AddWithValue("ProliferativeEyeLT", VerifyBooleanNull(eyeAndNeurology.ProliferativeEyeLT))
                    .AddWithValue("MacularEdema", VerifyBooleanNull(eyeAndNeurology.MacularEdema))
                    .AddWithValue("MacularEdemaEyeRT", VerifyBooleanNull(eyeAndNeurology.MacularEdemaEyeRT))
                    .AddWithValue("MacularEdemaEyeLT", VerifyBooleanNull(eyeAndNeurology.MacularEdemaEyeLT))
                    .AddWithValue("OtherComplicationRetinopathy", VerifyStringNull(eyeAndNeurology.OtherComplicationRetinopathy))
                    .AddWithValue("Glaucoma", VerifyBooleanNull(eyeAndNeurology.Glaucoma))
                    .AddWithValue("GlaucomaEyeRT", VerifyBooleanNull(eyeAndNeurology.GlaucomaEyeRT))
                    .AddWithValue("GlaucomaEyeLT", VerifyBooleanNull(eyeAndNeurology.GlaucomaEyeLT))
                    .AddWithValue("GlaucomaType", VerifyStringNull(eyeAndNeurology.GlaucomaType))
                    .AddWithValue("Cataract", VerifyBooleanNull(eyeAndNeurology.Cataract))
                    .AddWithValue("CataractRT", VerifyBooleanNull(eyeAndNeurology.CataractRT))
                    .AddWithValue("CataractLT", VerifyBooleanNull(eyeAndNeurology.CataractLT))
                    .AddWithValue("CataractType", VerifyStringNull(eyeAndNeurology.CataractType))
                    .AddWithValue("Epilepsy", VerifyBooleanNull(eyeAndNeurology.Epilepsy))
                    .AddWithValue("EpilepsyType", VerifyStringNull(eyeAndNeurology.EpilepsyType))
                    .AddWithValue("Seizures", VerifyBooleanNull(eyeAndNeurology.Seizures))
                    .AddWithValue("SeizuresCause", VerifyStringNull(eyeAndNeurology.SeizuresCause))
                    .AddWithValue("Polyneuropathy", VerifyBooleanNull(eyeAndNeurology.Polyneuropathy))
                    .AddWithValue("PolyneuropathyDueTo", VerifyStringNull(eyeAndNeurology.PolyneuropathyDueTo))
                    .AddWithValue("Neuropathy", VerifyBooleanNull(eyeAndNeurology.Neuropathy))
                    .AddWithValue("AutonomicNeuropathy", VerifyBooleanNull(eyeAndNeurology.AutonomicNeuropathy))
                    .AddWithValue("Mononeuritis", VerifyBooleanNull(eyeAndNeurology.Mononeuritis))
                    .AddWithValue("Neuralgia", VerifyBooleanNull(eyeAndNeurology.Neuralgia))
                    .AddWithValue("PolyneuropathyOtherSpecification", VerifyStringNull(eyeAndNeurology.PolyneuropathyOtherSpecification))
                    .AddWithValue("RetinopathyTreatmentPlan", VerifyStringNull(eyeAndNeurology.RetinopathyTreatmentPlan))
                    .AddWithValue("GlaucomaTreatmentPlan", VerifyStringNull(eyeAndNeurology.GlaucomaTreatmentPlan))
                    .AddWithValue("CataractTreatmentPlan", VerifyStringNull(eyeAndNeurology.CataractTreatmentPlan))
                    .AddWithValue("EpilepsyTreatmentPlan", VerifyStringNull(eyeAndNeurology.EpilepsyTreatmentPlan))
                    .AddWithValue("PolyneuropathyTreatmentPlan", VerifyStringNull(eyeAndNeurology.PolyneuropathyTreatmentPlan))
                    .AddWithValue("PolyneuropathyDueToCkb", VerifyBooleanNull(eyeAndNeurology.PolyneuropathyDueToCkb))

                    'ADD 2026 Changes'


                    .AddWithValue("RetinopathyEyeRT", VerifyBooleanNull(eyeAndNeurology.RetinopathyEyeRT))
                    .AddWithValue("RetinopathyEyeLT", VerifyBooleanNull(eyeAndNeurology.RetinopathyEyeLT))
                    .AddWithValue("AlzheimerDisease", VerifyBooleanNull(eyeAndNeurology.AlzheimerDisease))
                    .AddWithValue("Dementia", VerifyBooleanNull(eyeAndNeurology.Dementia))


                    .AddWithValue("RetinopathySeverity", VerifyIntegerNull(eyeAndNeurology.RetinopathySeverity))
                    .AddWithValue("ProliferativeSeverity", VerifyIntegerNull(eyeAndNeurology.ProliferativeSeverity))
                    .AddWithValue("ProliferativeTreatmentPlan", VerifyStringNull(eyeAndNeurology.ProliferativeTreatmentPlan))
                    .AddWithValue("DementiaAlzheimerTreatmentPlan", VerifyStringNull(eyeAndNeurology.DementiaAlzheimerTreatmentPlan))




                End With

                dbo.ExecuteCommand(cmd)

                If eyeAndNeurology IsNot Nothing AndAlso eyeAndNeurology.MedicationList IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "neuro", eyeAndNeurology.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If
            End Using

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally

        End Try

    End Sub

    Private Sub SaveEyeAndNeurology2023(ByVal claimKey As Long, ByVal eyeAndNeurology As EyesAndNeurology, ByVal dbo As SqlDataObject)

        Try
            If eyeAndNeurology Is Nothing Then eyeAndNeurology = New EyesAndNeurology()

            'If Not IsNothing(eyeAndNeurology.NA) Then
            '    If Not IsNothing(eyeAndNeurology.NA.Value) AndAlso eyeAndNeurology.NA.Value Then
            '        eyeAndNeurology = New EyesAndNeurology()
            '    End If
            'End If
            Using cmd As New SqlCommand

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveEyeAndNeurology2023"

                With cmd.Parameters

                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("EyesAndNeurologyNA", VerifyBooleanNull(eyeAndNeurology.NA))



                    .AddWithValue("MacularEdema", VerifyBooleanNull(eyeAndNeurology.MacularEdema))
                    .AddWithValue("MacularEdemaEyeRT", VerifyBooleanNull(eyeAndNeurology.MacularEdemaEyeRT))
                    .AddWithValue("MacularEdemaEyeLT", VerifyBooleanNull(eyeAndNeurology.MacularEdemaEyeLT))
                    .AddWithValue("OtherComplicationRetinopathy", VerifyStringNull(eyeAndNeurology.OtherComplicationRetinopathy))
                    .AddWithValue("Glaucoma", VerifyBooleanNull(eyeAndNeurology.Glaucoma))
                    .AddWithValue("GlaucomaEyeRT", VerifyBooleanNull(eyeAndNeurology.GlaucomaEyeRT))
                    .AddWithValue("GlaucomaEyeLT", VerifyBooleanNull(eyeAndNeurology.GlaucomaEyeLT))
                    .AddWithValue("GlaucomaType", VerifyStringNull(eyeAndNeurology.GlaucomaType))
                    .AddWithValue("Cataract", VerifyBooleanNull(eyeAndNeurology.Cataract))
                    .AddWithValue("CataractRT", VerifyBooleanNull(eyeAndNeurology.CataractRT))
                    .AddWithValue("CataractLT", VerifyBooleanNull(eyeAndNeurology.CataractLT))
                    .AddWithValue("CataractType", VerifyStringNull(eyeAndNeurology.CataractType))
                    .AddWithValue("Epilepsy", VerifyBooleanNull(eyeAndNeurology.Epilepsy))
                    .AddWithValue("EpilepsyType", VerifyStringNull(eyeAndNeurology.EpilepsyType))
                    .AddWithValue("Seizures", VerifyBooleanNull(eyeAndNeurology.Seizures))
                    .AddWithValue("SeizuresCause", VerifyStringNull(eyeAndNeurology.SeizuresCause))
                    .AddWithValue("Polyneuropathy", VerifyBooleanNull(eyeAndNeurology.Polyneuropathy))
                    .AddWithValue("PolyneuropathyDueTo", VerifyStringNull(eyeAndNeurology.PolyneuropathyDueTo))
                    .AddWithValue("Neuropathy", VerifyBooleanNull(eyeAndNeurology.Neuropathy))
                    .AddWithValue("AutonomicNeuropathy", VerifyBooleanNull(eyeAndNeurology.AutonomicNeuropathy))
                    .AddWithValue("Mononeuritis", VerifyBooleanNull(eyeAndNeurology.Mononeuritis))
                    .AddWithValue("Neuralgia", VerifyBooleanNull(eyeAndNeurology.Neuralgia))
                    .AddWithValue("PolyneuropathyOtherSpecification", VerifyStringNull(eyeAndNeurology.PolyneuropathyOtherSpecification))
                    .AddWithValue("RetinopathyTreatmentPlan", VerifyStringNull(eyeAndNeurology.RetinopathyTreatmentPlan))
                    .AddWithValue("GlaucomaTreatmentPlan", VerifyStringNull(eyeAndNeurology.GlaucomaTreatmentPlan))
                    .AddWithValue("CataractTreatmentPlan", VerifyStringNull(eyeAndNeurology.CataractTreatmentPlan))
                    .AddWithValue("EpilepsyTreatmentPlan", VerifyStringNull(eyeAndNeurology.EpilepsyTreatmentPlan))
                    .AddWithValue("PolyneuropathyTreatmentPlan", VerifyStringNull(eyeAndNeurology.PolyneuropathyTreatmentPlan))
                    .AddWithValue("PolyneuropathyDueToCkb", VerifyBooleanNull(eyeAndNeurology.PolyneuropathyDueToCkb))


                    'ADD 2026 Changes'
                    .AddWithValue("Retinopathy", VerifyBooleanNull(eyeAndNeurology.Retinopathy))
                    .AddWithValue("RetinopathyEyeRT", VerifyBooleanNull(eyeAndNeurology.RetinopathyEyeRT))
                    .AddWithValue("RetinopathyEyeLT", VerifyBooleanNull(eyeAndNeurology.RetinopathyEyeLT))

                    .AddWithValue("Proliferative", VerifyBooleanNull(eyeAndNeurology.Proliferative))
                    .AddWithValue("ProliferativeEyeRT", VerifyBooleanNull(eyeAndNeurology.ProliferativeEyeRT))
                    .AddWithValue("ProliferativeEyeLT", VerifyBooleanNull(eyeAndNeurology.ProliferativeEyeLT))

                    .AddWithValue("AlzheimerDisease", VerifyBooleanNull(eyeAndNeurology.AlzheimerDisease))
                    .AddWithValue("Dementia", VerifyBooleanNull(eyeAndNeurology.Dementia))


                    .AddWithValue("RetinopathySeverity", VerifyIntegerNull(eyeAndNeurology.RetinopathySeverity))
                    .AddWithValue("ProliferativeSeverity", VerifyIntegerNull(eyeAndNeurology.ProliferativeSeverity))
                    .AddWithValue("ProliferativeTreatmentPlan", VerifyStringNull(eyeAndNeurology.ProliferativeTreatmentPlan))

                    .AddWithValue("DementiaAlzheimerTreatmentPlan", VerifyStringNull(eyeAndNeurology.DementiaAlzheimerTreatmentPlan))
                    .AddWithValue("DementiaSeverity", VerifyIntegerNull(eyeAndNeurology.DementiaSeverity))


                End With

                dbo.ExecuteCommand(cmd)

                If eyeAndNeurology IsNot Nothing AndAlso eyeAndNeurology.MedicationList IsNot Nothing Then
                    'Dim s As Boolean = SaveMedicationbySection(claimKey, "neuro", eyeAndNeurology.MedicationList, dbo)
                    'manage if error ocurred exaption is been handled
                End If
            End Using

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally

        End Try

    End Sub


    Private Sub SaveGastrointestinalDiseases(ByVal claimKey As Long, ByVal gastrointestinal As GastrointestinalDiseases, ByVal dbo As SqlDataObject)
        If gastrointestinal Is Nothing Then
            gastrointestinal = New GastrointestinalDiseases With {.NA = New BooleanField}

        End If

        Dim cmd As New SqlClient.SqlCommand


        Try
            If gastrointestinal IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveGastrointestinal"

                With cmd.Parameters


                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NA", VerifyBooleanNull(gastrointestinal.NA))
                    .AddWithValue("NASH", VerifyBooleanNull(gastrointestinal.NonalcoholicSteatohepatitis))
                    .AddWithValue("MetabolicSyndrome", VerifyBooleanNull(gastrointestinal.MetabolicSyndrome))
                    .AddWithValue("Hyperkalemia", VerifyBooleanNull(gastrointestinal.Hyperkalemia))
                    .AddWithValue("Hypokalemia", VerifyBooleanNull(gastrointestinal.Hypokalemia))
                    .AddWithValue("TreatmentPlan", VerifyStringNull(gastrointestinal.GastrointestinalTreatmentPlan))
                    .AddWithValue("LiverTransplant", VerifyBooleanNull(gastrointestinal.LiverTransplant))

                    .AddWithValue("GERD", VerifyBooleanNull(gastrointestinal.GERD))
                    .AddWithValue("ChronicHepatitis", VerifyBooleanNull(gastrointestinal.ChronicHepatitis))
                    .AddWithValue("DiverticularDisease", VerifyBooleanNull(gastrointestinal.DiverticularDisease))
                    .AddWithValue("PepticUlcerDisease", VerifyBooleanNull(gastrointestinal.PepticUlcerDisease))




                End With

                dbo.ExecuteCommand(cmd)
            End If


        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SavePulmonaryDiseases(ByVal claimKey As Long, ByVal pd As PulmonaryDiseases, ByVal dbo As SqlDataObject)

        If pd Is Nothing Then
            pd = New PulmonaryDiseases With {.NA = New BooleanField}
        End If

        Dim cmd As New SqlClient.SqlCommand

        Try
            If pd IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSavePulmonaryDiseases"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NA", VerifyBooleanNull(pd.NA))
                    .AddWithValue("Asthma", VerifyBooleanNull(pd.Asthma))
                    .AddWithValue("AsthmaComments", VerifyStringNull(pd.AsthmaComments))
                    .AddWithValue("AsthmaDescription", VerifyStringNull(pd.AsthmaDescription))
                    .AddWithValue("AcuteBronchitis", VerifyBooleanNull(pd.AcuteBronchitis))
                    .AddWithValue("AcuteBronchitisComments", VerifyStringNull(pd.AcuteBronchitisComments))
                    .AddWithValue("ChronicBronchitis", VerifyBooleanNull(pd.ChronicBronchitis))
                    .AddWithValue("ChronicBronchitisComments", VerifyStringNull(pd.ChronicBronchitisComments))
                    .AddWithValue("COPD", VerifyBooleanNull(pd.COPD))
                    .AddWithValue("COPDComments", VerifyStringNull(pd.COPDComments))
                    .AddWithValue("PulmonaryFibrosis", VerifyBooleanNull(pd.PulmonaryFibrosis))
                    .AddWithValue("PulmonaryFibrosisComments", VerifyStringNull(pd.PulmonaryFibrosisComments))
                    .AddWithValue("AcuteLaryngopharyngitis", VerifyBooleanNull(pd.AcuteLaryngopharyngitis))
                    .AddWithValue("AcuteLaryngopharyngitisComments", VerifyStringNull(pd.AcuteLaryngopharyngitisComments))
                    .AddWithValue("AcuteNasopharyngitis", VerifyBooleanNull(pd.AcuteNasopharyngitis))
                    .AddWithValue("AcuteNasopharyngitisComments", VerifyStringNull(pd.AcuteNasopharyngitisComments))
                    .AddWithValue("UpperRespiratoryTractInfection", VerifyBooleanNull(pd.UpperRespiratoryTractInfection))
                    .AddWithValue("UpperRespiratoryTractInfectionComments", VerifyStringNull(pd.UpperRespiratoryTractInfectionComments))

                    .AddWithValue("PulmonaryDiseases_OtherCondition_Checkbox", VerifyBooleanNull(pd.PulmonaryDiseases_OtherCondition_Checkbox))
                    .AddWithValue("PulmonaryDiseases_OtherCondition", VerifyStringNull(pd.PulmonaryDiseases_OtherCondition))
                    .AddWithValue("PulmonaryDiseases_OtherConditionTreatment", VerifyStringNull(pd.PulmonaryDiseases_OtherConditionTreatment))


                    'ADD 2026 Changes'
                    .AddWithValue("LungTransplant", VerifyBooleanNull(pd.LungTransplant))
                    .AddWithValue("LungTransplantTreatmentPlan", VerifyStringNull(pd.LungTransplantTreatmentPlan))


                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub



    Private Sub SaveCongenitalDiseases(ByVal claimKey As Long, ByVal congenitalDiseasesSec As CongenitalDiseases,
                                    ByVal dbo As SqlDataObject)

        Dim cmd As New SqlClient.SqlCommand

        Try

            If IsNothing(congenitalDiseasesSec) Then
                congenitalDiseasesSec = New CongenitalDiseases()
            End If

            If congenitalDiseasesSec IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveCongenitalDiseases"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)

                    .AddWithValue("CongenitalDiseases_NA", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_NA))

                    If congenitalDiseasesSec.CongenitalDiseases_SpinaBifida IsNot Nothing Then .AddWithValue("CongenitalDiseases_SpinaBifida", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_SpinaBifida))
                    If congenitalDiseasesSec.CongenitalDiseases_SpinaBifidaComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_SpinaBifidaComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_SpinaBifidaComments))

                    If congenitalDiseasesSec.CongenitalDiseases_Hydrocephalus IsNot Nothing Then .AddWithValue("CongenitalDiseases_Hydrocephalus", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_Hydrocephalus))
                    If congenitalDiseasesSec.CongenitalDiseases_HydrocephalusComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_HydrocephalusComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_HydrocephalusComments))

                    If congenitalDiseasesSec.CongenitalDiseases_ChiariMalformation IsNot Nothing Then .AddWithValue("CongenitalDiseases_ChiariMalformation", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_ChiariMalformation))
                    If congenitalDiseasesSec.CongenitalDiseases_ChiariMalformationComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_ChiariMalformationComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_ChiariMalformationComments))

                    If congenitalDiseasesSec.CongenitalDiseases_Hemophilia IsNot Nothing Then .AddWithValue("CongenitalDiseases_Hemophilia", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_Hemophilia))
                    If congenitalDiseasesSec.CongenitalDiseases_HemophiliaComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_HemophiliaComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_HemophiliaComments))

                    If congenitalDiseasesSec.CongenitalDiseases_Cranofacial IsNot Nothing Then .AddWithValue("CongenitalDiseases_Cranofacial", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_Cranofacial))
                    If congenitalDiseasesSec.CongenitalDiseases_CranofacialComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_CranofacialComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_CranofacialComments))

                    If congenitalDiseasesSec.CongenitalDiseases_DistrofiaMuscular IsNot Nothing Then .AddWithValue("CongenitalDiseases_DistrofiaMuscular", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_DistrofiaMuscular))
                    If congenitalDiseasesSec.CongenitalDiseases_DistrofiaMuscularComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_DistrofiaMuscularComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_DistrofiaMuscularComments))

                    If congenitalDiseasesSec.CongenitalDiseases_CerebralPalsy IsNot Nothing Then .AddWithValue("CongenitalDiseases_CerebralPalsy", VerifyBooleanNull(congenitalDiseasesSec.CongenitalDiseases_CerebralPalsy))
                    If congenitalDiseasesSec.CongenitalDiseases_CerebralPalsyText IsNot Nothing Then .AddWithValue("CongenitalDiseases_CerebralPalsyText", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_CerebralPalsyText))
                    If congenitalDiseasesSec.CongenitalDiseases_CerebralPalsyComments IsNot Nothing Then .AddWithValue("CongenitalDiseases_CerebralPalsyComments", VerifyStringNull(congenitalDiseasesSec.CongenitalDiseases_CerebralPalsyComments))

                End With

                dbo.ExecuteCommand(cmd)

            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try

    End Sub

    Private Sub SaveGastrointestinal(ByVal claimKey As Long, ByVal gt As GastrointestinalSection, ByVal dbo As SqlDataObject)
        If gt Is Nothing Then
            gt = New GastrointestinalSection With {.NA = New BooleanField}
        End If

        Dim cmd As New SqlClient.SqlCommand

        Try
            If gt IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveGastrointestinal"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NA", VerifyBooleanNull(gt.NA))
                    .AddWithValue("NASH", VerifyBooleanNull(gt.NASH))
                    .AddWithValue("MetabolicSyndrome", VerifyBooleanNull(gt.MetabolicSyndrome))
                    .AddWithValue("Hyperkalemia", VerifyBooleanNull(gt.Hyperkalemia))
                    .AddWithValue("Hypokalemia", VerifyBooleanNull(gt.Hypokalemia))
                    .AddWithValue("TreatmentPlan", VerifyStringNull(gt.GastrointestinalTreatmentPlan))
                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try
    End Sub

    Private Sub SaveMusculoskeletal(ByVal claimKey As Long, ByVal mk As MusculoskeletalSection, ByVal dbo As SqlDataObject)
        If mk Is Nothing Then
            mk = New MusculoskeletalSection With {.NA = New BooleanField}
        End If

        Dim cmd As New SqlClient.SqlCommand

        Try
            If mk IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMusculoskeletal"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("NA", VerifyBooleanNull(mk.NA))
                    .AddWithValue("Spondylosis", VerifyBooleanNull(mk.Spondylosis))
                    .AddWithValue("CervicalDiscDisorder", VerifyBooleanNull(mk.CervicalDiscDisorder))
                    .AddWithValue("CervicothoracicRadiculopathy", VerifyBooleanNull(mk.CervicothoracicRadiculopathy))
                    .AddWithValue("CervicalRegion", VerifyBooleanNull(mk.CervicalRegion))
                    .AddWithValue("CervicothoracicRegion", VerifyBooleanNull(mk.CervicothoracicRegion))
                    .AddWithValue("TreatmentPlan", VerifyStringNull(mk.MusculoskeletalTreatmentPlan))
                End With

                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimKey.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try
    End Sub


    Public Function SaveAHADxHistSelection(ByVal claimID As Long, ByVal ahaDxHxSelectionList As List(Of AHADxHxSelection)) As Boolean

        Dim dxHistTable As New DataTable

        Try

            If ahaDxHxSelectionList Is Nothing Then
                ahaDxHxSelectionList = New List(Of AHADxHxSelection)()

            End If

            With dxHistTable.Columns
                .Add("ClaimID", System.Type.GetType("System.Int64"))
                .Add("DxCode", System.Type.GetType("System.String"))
                .Add("DxDescription", System.Type.GetType("System.String"))
                .Add("ProviderName", System.Type.GetType("System.String"))
                .Add("Source", System.Type.GetType("System.String"))
                .Add("SelectionIndex", System.Type.GetType("System.Int32"))
                .Add("ReasonForNo", System.Type.GetType("System.Int16"))
            End With

            Dim drDx As DataRow

            For Each dx As AHADxHxSelection In ahaDxHxSelectionList

                drDx = dxHistTable.NewRow()

                drDx("ClaimID") = claimID
                drDx("DxCode") = dx.DxCode
                drDx("DxDescription") = dx.DxDescription
                drDx("ProviderName") = dx.ProviderName
                drDx("Source") = dx.Source
                drDx("SelectionIndex") = dx.SelectionIndex
                drDx("ReasonForNo") = dx.ReasonForNo
                dxHistTable.Rows.Add(drDx)

            Next

            Dim param As New SqlClient.SqlParameter("AHADxHistSelectionTable", SqlDbType.Structured)
            param.Value = dxHistTable

            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)
            Dim cmd As New SqlClient.SqlCommand

            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = "uspAHA_SaveDxHistorySelection"

            cmd.Parameters.AddWithValue("ClaimID", claimID)
            cmd.Parameters.Add(param)


            Dim affectedRecords As Integer = _DBO.ExecuteCommand(cmd)

            If affectedRecords > 0 Then
                Return True
            End If

        Catch ex As Exception
            Throw
        End Try

        Return False

    End Function

    Private Function CreateBarCode() As String
        Dim dbo As SqlDataObject = GetSqlObject()

        Dim last As String = dbo.GetSingleValue("uspGetIdentity 'BCODE', 99999")

        Dim sBarCode As String

        sBarCode = Right(Now.Year.ToString, 2)
        sBarCode += Now.Month.ToString.PadLeft(2, "0")
        sBarCode += Now.Day.ToString.PadLeft(2, "0")
        sBarCode += last.PadLeft(5, "0")

        sBarCode += "0"

        dbo.Dispose()

        Return sBarCode

    End Function


    Public Function SaveMedicationbySection(ByVal claimKey As Long, ByVal SectionID As String, ByVal medicationList_section As List(Of String), ByVal dbo As SqlDataObject) As Boolean

        Dim cmd As New SqlClient.SqlCommand


        Try
            If medicationList_section IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveMedicationBySection"

                With cmd.Parameters
                    .AddWithValue("ClaimID", claimKey)
                    .AddWithValue("SectionID", SectionID)

                    Dim MedListTable As New DataTable
                    MedListTable.Columns.Add("MedicationName", Type.GetType("System.String"))
                    MedListTable.Columns.Add("IsAdherence", Type.GetType("System.Boolean"))

                    For Each m In medicationList_section

                        Dim drMed As DataRow = MedListTable.NewRow

                        drMed("MedicationName") = m.Trim.ToString
                        drMed("IsAdherence") = False

                        MedListTable.Rows.Add(drMed)

                    Next
                    Dim param As New SqlParameter("MedList", SqlDbType.Structured)
                    param.Value = MedListTable

                    .Add(param)

                End With

                dbo.ExecuteCommand(cmd)

            End If

        Catch ex As Exception
            Throw
        End Try

        Return False


    End Function

    'Public Function SaveClaimsSocialDeterminants(ByVal claimID As Long, ByVal socialdeterminants As SocialDeterminants2020, ByVal dbo As SqlDataObject) As Boolean
    Public Function SaveClaimsSocialDeterminants(ByVal claimID As Long, ByVal socialdeterminants As SocialDeterminants2020, ByVal socialdeterminantsna As BooleanField, ByVal dbo As SqlDataObject) As Boolean

        Dim cmd As New SqlClient.SqlCommand

        Try

            If IsNothing(socialdeterminants) Then
                socialdeterminants = New SocialDeterminants2020()
            End If

            If socialdeterminants IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveClaims_SocialDeterminants"
                With cmd.Parameters
                    .AddWithValue("biClaimID", claimID)
                    .AddWithValue("problems_living_alone", IIf(socialdeterminants.problems_living_alone?.Value Is Nothing, False, socialdeterminants.problems_living_alone?.Value))
                    .AddWithValue("illiteracy", IIf(socialdeterminants.illiteracy?.Value Is Nothing, False, socialdeterminants.illiteracy?.Value))
                    .AddWithValue("homelessness", IIf(socialdeterminants.homelessness?.Value Is Nothing, False, socialdeterminants.homelessness?.Value))
                    .AddWithValue("inadequate_home", IIf(socialdeterminants.inadequate_home?.Value Is Nothing, False, socialdeterminants.inadequate_home?.Value))
                    .AddWithValue("discord_with_nll", IIf(socialdeterminants.discord_with_nll?.Value Is Nothing, False, socialdeterminants.discord_with_nll?.Value))
                    .AddWithValue("problems_residential_institution", IIf(socialdeterminants.problems_residential_institution?.Value Is Nothing, False, socialdeterminants.problems_residential_institution?.Value))
                    .AddWithValue("lack_of_food_and_water", IIf(socialdeterminants.lack_of_food_and_water?.Value Is Nothing, False, socialdeterminants.lack_of_food_and_water?.Value))
                    .AddWithValue("extreme_poverty", IIf(socialdeterminants.extreme_poverty?.Value Is Nothing, False, socialdeterminants.extreme_poverty?.Value))
                    .AddWithValue("worried_about_losing_housing", IIf(socialdeterminants.worried_about_losing_housing?.Value Is Nothing, False, socialdeterminants.worried_about_losing_housing?.Value))
                    .AddWithValue("not_able_to_pay_rx", IIf(socialdeterminants.not_able_to_pay_rx?.Value Is Nothing, False, socialdeterminants.not_able_to_pay_rx?.Value))
                    .AddWithValue("not_able_to_pay_utilities", IIf(socialdeterminants.not_able_to_pay_utilities?.Value Is Nothing, False, socialdeterminants.not_able_to_pay_utilities?.Value))
                    .AddWithValue("not_able_to_pay_medical_care", IIf(socialdeterminants.not_able_to_pay_medical_care?.Value Is Nothing, False, socialdeterminants.not_able_to_pay_medical_care?.Value))
                    .AddWithValue("not_able_to_pay_phone", IIf(socialdeterminants.not_able_to_pay_phone?.Value Is Nothing, False, socialdeterminants.not_able_to_pay_phone?.Value))
                    .AddWithValue("not_able_to_pay_transportation", IIf(socialdeterminants.not_able_to_pay_transportation?.Value Is Nothing, False, socialdeterminants.not_able_to_pay_transportation?.Value))
                    .AddWithValue("not_able_to_pay_clothing", IIf(socialdeterminants.not_able_to_pay_clothing?.Value Is Nothing, False, socialdeterminants.not_able_to_pay_clothing?.Value))
                    .AddWithValue("problems_in_relationship", IIf(socialdeterminants.problems_in_relationship?.Value Is Nothing, False, socialdeterminants.problems_in_relationship?.Value))
                    .AddWithValue("absence_family_member_military", IIf(socialdeterminants.absence_family_member_military?.Value Is Nothing, False, socialdeterminants.absence_family_member_military?.Value))
                    .AddWithValue("disappearance_family_member", IIf(socialdeterminants.disappearance_family_member?.Value Is Nothing, False, socialdeterminants.disappearance_family_member?.Value))
                    .AddWithValue("other_absence_family_member", IIf(socialdeterminants.other_absence_family_member?.Value Is Nothing, False, socialdeterminants.other_absence_family_member?.Value))
                    .AddWithValue("disruption_separation", IIf(socialdeterminants.disruption_separation?.Value Is Nothing, False, socialdeterminants.disruption_separation?.Value))
                    .AddWithValue("dependent_at_home", IIf(socialdeterminants.dependent_at_home?.Value Is Nothing, False, socialdeterminants.dependent_at_home?.Value))
                    .AddWithValue("alcoholism_drug_addiction_family", IIf(socialdeterminants.alcoholism_drug_addiction_family?.Value Is Nothing, False, socialdeterminants.alcoholism_drug_addiction_family?.Value))
                    .AddWithValue("innapropriate_diet", IIf(socialdeterminants.innapropriate_diet?.Value Is Nothing, False, socialdeterminants.innapropriate_diet?.Value))
                    .AddWithValue("other_reduced_mobility", IIf(socialdeterminants.other_reduced_mobility?.Value Is Nothing, False, socialdeterminants.other_reduced_mobility?.Value))
                    .AddWithValue("need_personal_care", IIf(socialdeterminants.need_personal_care?.Value Is Nothing, False, socialdeterminants.need_personal_care?.Value))
                    .AddWithValue("need_at_home", IIf(socialdeterminants.need_at_home?.Value Is Nothing, False, socialdeterminants.need_at_home?.Value))
                    .AddWithValue("need_continuous_supervision", IIf(socialdeterminants.need_continuous_supervision?.Value Is Nothing, False, socialdeterminants.need_continuous_supervision?.Value))
                    .AddWithValue("other_problems_provider_dependency", IIf(socialdeterminants.other_problems_provider_dependency?.Value Is Nothing, False, socialdeterminants.other_problems_provider_dependency?.Value))
                    .AddWithValue("unavailability_other_helping_agencies", IIf(socialdeterminants.unavailability_other_helping_agencies?.Value Is Nothing, False, socialdeterminants.unavailability_other_helping_agencies?.Value))
                    .AddWithValue("need_assisstance_daily_activities", IIf(socialdeterminants.need_assisstance_daily_activities?.Value Is Nothing, False, socialdeterminants.need_assisstance_daily_activities?.Value))
                    .AddWithValue("bedridden_few_to_no_resources", IIf(socialdeterminants.bedridden_few_to_no_resources?.Value Is Nothing, False, socialdeterminants.bedridden_few_to_no_resources?.Value))
                    .AddWithValue("partialy_depends_no_resource", IIf(socialdeterminants.partialy_depends_no_resource?.Value Is Nothing, False, socialdeterminants.partialy_depends_no_resource?.Value))

                    .AddWithValue("socialdeterminants_na", IIf(socialdeterminantsna?.Value Is Nothing, False, socialdeterminantsna?.Value))

                End With


                dbo.ExecuteCommand(cmd)



            End If




        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimID.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try
    End Function

    Public Function SaveClaimsMalnutritionCriteria(ByVal claimID As Long, ByVal malnutritioncriteria As MalnutritionCriteria, ByVal dbo As SqlDataObject) As Boolean
        Dim cmd As New SqlClient.SqlCommand

        Try

            If IsNothing(malnutritioncriteria) Then
                malnutritioncriteria = New MalnutritionCriteria()
            End If

            If malnutritioncriteria IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveClaims_MalnutritionCriteria"
                With cmd.Parameters
                    .AddWithValue("biClaimID", claimID)
                    .AddWithValue("involuntary_weight_loss", IIf(malnutritioncriteria?.involuntary_weight_loss?.Value Is Nothing, False, malnutritioncriteria?.involuntary_weight_loss?.Value))
                    .AddWithValue("involuntary_weight_loss10to5at6monthand20to10over6month", IIf(malnutritioncriteria?.involuntary_weight_loss10to5at6monthand20to10over6month?.Value Is Nothing, False, malnutritioncriteria?.involuntary_weight_loss10to5at6monthand20to10over6month?.Value))
                    .AddWithValue("involuntary_weight_lossless5at6monthandless10over6month", IIf(malnutritioncriteria.involuntary_weight_lossless5at6monthandless10over6month?.Value Is Nothing, False, malnutritioncriteria?.involuntary_weight_lossless5at6monthandless10over6month?.Value))
                    .AddWithValue("involuntary_weight_lossmore10at6monthandmore20over6month", IIf(malnutritioncriteria.involuntary_weight_lossmore10at6monthandmore20over6month?.Value Is Nothing, False, malnutritioncriteria?.involuntary_weight_lossmore10at6monthandmore20over6month?.Value))
                    .AddWithValue("Low_bmi", IIf(malnutritioncriteria.Low_bmi?.Value Is Nothing, False, malnutritioncriteria.Low_bmi?.Value))
                    .AddWithValue("low_bmiless18", IIf(malnutritioncriteria.low_bmiless18?.Value Is Nothing, False, malnutritioncriteria.low_bmiless18?.Value))
                    .AddWithValue("low_bmiless20", IIf(malnutritioncriteria.low_bmiless20?.Value Is Nothing, False, malnutritioncriteria.low_bmiless20?.Value))
                    .AddWithValue("reduced_muscle", IIf(malnutritioncriteria.reduced_muscle?.Value Is Nothing, False, malnutritioncriteria.reduced_muscle?.Value))
                    .AddWithValue("reduced_muscle_severly", IIf(malnutritioncriteria.reduced_muscle_severly?.Value Is Nothing, False, malnutritioncriteria.reduced_muscle_severly?.Value))
                    .AddWithValue("reduced_muscle_mild", IIf(malnutritioncriteria.reduced_muscle_mild?.Value Is Nothing, False, malnutritioncriteria.reduced_muscle_mild?.Value))
                    .AddWithValue("reduced_food_intake", IIf(malnutritioncriteria.reduced_food_intake?.Value Is Nothing, False, malnutritioncriteria.reduced_food_intake?.Value))
                    .AddWithValue("disease_burden", IIf(malnutritioncriteria.disease_burden?.Value Is Nothing, False, malnutritioncriteria.disease_burden?.Value))

                    .AddWithValue("other_criteria", IIf(malnutritioncriteria.other_criteria?.Value Is Nothing, False, malnutritioncriteria.other_criteria?.Value))
                    .AddWithValue("other_criteria_description", IIf(malnutritioncriteria.other_criteria_description?.Value Is Nothing, "", malnutritioncriteria.other_criteria_description?.Value))

                    .AddWithValue("albumin", IIf(malnutritioncriteria.albumin?.Value Is Nothing, False, malnutritioncriteria.albumin?.Value))
                    .AddWithValue("less2albumin", IIf(malnutritioncriteria.less2albumin?.Value Is Nothing, False, malnutritioncriteria.less2albumin?.Value))
                    .AddWithValue("less25albumin", IIf(malnutritioncriteria.less25albumin?.Value Is Nothing, False, malnutritioncriteria.less25albumin?.Value))
                    .AddWithValue("less35albumin", IIf(malnutritioncriteria.less35albumin?.Value Is Nothing, False, malnutritioncriteria.less35albumin?.Value))
                End With
                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimID.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try
    End Function
    Public Function SaveClaimsScreeningSubstanceUse(ByVal claimID As Long, ByVal ScreeningSubstanceUseList As List(Of ScreeningSubstanceUse), ByVal dbo As SqlDataObject) As Boolean
        Dim cmd As New SqlClient.SqlCommand

        Try

            If IsNothing(ScreeningSubstanceUseList) Then
                ScreeningSubstanceUseList = New List(Of ScreeningSubstanceUse)
            End If

            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = "uspDeleteClaims_ScreeningSubstanceUse"
            With cmd.Parameters
                .AddWithValue("biClaimID", claimID)
            End With
            dbo.ExecuteCommand(cmd)


            If ScreeningSubstanceUseList IsNot Nothing Then

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveClaims_ScreeningSubstanceUse"

                For Each CriteriaItem In ScreeningSubstanceUseList
                    cmd.Parameters?.Clear()

                    With cmd.Parameters
                        .AddWithValue("biClaimID", claimID)
                        .AddWithValue("criteria_id", IIf(CriteriaItem.criteria_id.Value Is Nothing, False, CriteriaItem.criteria_id?.Value))
                        .AddWithValue("screening_substance_use_other", IIf(CriteriaItem.screening_substance_use_other?.Value Is Nothing, "", CriteriaItem.screening_substance_use_other?.Value))
                        .AddWithValue("screening_substance_use_q1", IIf(CriteriaItem.screening_substance_use_q1?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q1?.Value))
                        .AddWithValue("screening_substance_use_q2", IIf(CriteriaItem.screening_substance_use_q2?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q2?.Value))
                        .AddWithValue("screening_substance_use_q3", IIf(CriteriaItem.screening_substance_use_q3?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q3?.Value))
                        .AddWithValue("screening_substance_use_q4", IIf(CriteriaItem.screening_substance_use_q4?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q4?.Value))
                        .AddWithValue("screening_substance_use_q5", IIf(CriteriaItem.screening_substance_use_q5?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q5?.Value))
                        .AddWithValue("screening_substance_use_q6", IIf(CriteriaItem.screening_substance_use_q6?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q6?.Value))
                        .AddWithValue("screening_substance_use_q7", IIf(CriteriaItem.screening_substance_use_q7?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q7?.Value))
                        .AddWithValue("screening_substance_use_q8", IIf(CriteriaItem.screening_substance_use_q8?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q8?.Value))
                        .AddWithValue("screening_substance_use_q9", IIf(CriteriaItem.screening_substance_use_q9?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q9?.Value))
                        .AddWithValue("screening_substance_use_q10", IIf(CriteriaItem.screening_substance_use_q10?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q10?.Value))
                        .AddWithValue("screening_substance_use_q11", IIf(CriteriaItem.screening_substance_use_q11?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q11?.Value))
                        .AddWithValue("screening_substance_use_q12", IIf(CriteriaItem.screening_substance_use_q12?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q12?.Value))
                        .AddWithValue("screening_substance_use_q13", IIf(CriteriaItem.screening_substance_use_q13?.Value Is Nothing, False, CriteriaItem.screening_substance_use_q13?.Value))
                        .AddWithValue("screening_substance_use_total", IIf(CriteriaItem.screening_substance_use_total?.Value Is Nothing, False, CriteriaItem.screening_substance_use_total?.Value))
                    End With
                    dbo.ExecuteCommand(cmd)
                Next
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimID.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try
    End Function

    Public Function SaveScreeningResult(ByVal claimID As Long,
                                        ByVal ScreeningSubstanceUseListResult As String,
                                        ByVal ScreeningSocialDeterminants2020Result As String,
                                        ByVal ScreeningMalnutritionCriteriaResult As String,
                                        ByVal dbo As SqlDataObject) As Boolean


        Dim cmd As New SqlClient.SqlCommand

        Try

            cmd.CommandType = CommandType.StoredProcedure
            cmd.CommandText = "uspSaveScreeningResult"

            cmd.Parameters?.Clear()

            With cmd.Parameters
                .AddWithValue("ClaimID", claimID)
                .AddWithValue("ScreeningSubstanceUseListResult", IIf(ScreeningSubstanceUseListResult Is Nothing, "", ScreeningSubstanceUseListResult))
                .AddWithValue("ScreeningMalnutritionCriteriaResult", IIf(ScreeningMalnutritionCriteriaResult Is Nothing, "", ScreeningMalnutritionCriteriaResult))
                .AddWithValue("ScreeningSocialDeterminants2020Result", IIf(ScreeningSocialDeterminants2020Result Is Nothing, "", ScreeningSocialDeterminants2020Result))
            End With

            dbo.ExecuteCommand(cmd)

        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimID.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try


    End Function



#Region "Log Session"
    Public Function LogSession(ByVal portalSession As Models.PortalSession) As Long
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "uspSessions_Add"

                    cmd.Parameters.AddWithValue("@sAccountNo", If(portalSession.AccountNo Is Nothing, "", portalSession.AccountNo))
                    cmd.Parameters.AddWithValue("@sLoginID", If(portalSession.LoginID Is Nothing, "", portalSession.LoginID))
                    cmd.Parameters.AddWithValue("@sToken", If(portalSession.Token Is Nothing, "", portalSession.Token))
                    cmd.Parameters.AddWithValue("@sSourceIP", portalSession.SourceIP)
                    cmd.Parameters.AddWithValue("@sApplicationUser", portalSession.ApplicationUser)
                    cmd.Parameters.AddWithValue("@sPortalID", portalSession.PortalID)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    Dim portalSessionID As Long = Convert.ToInt64(cmd.ExecuteScalar)

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return portalSessionID
                End Using
            End Using
        Catch
            Throw
        End Try
    End Function

#End Region


    Public Function LogAckowledgement(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "usp_InserWelcomeLetterAckonledgeLog"

                    cmd.Parameters.AddWithValue("RenderingNPI", RenderingNPI)
                    cmd.Parameters.AddWithValue("Year", Year)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    cmd.ExecuteNonQuery()

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return True
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetLogAckowledgement(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "usp_GetAcknowledgementByRenderingNPI"

                    cmd.Parameters.AddWithValue("RenderingNPI", RenderingNPI)
                    cmd.Parameters.AddWithValue("Year", Year)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    Dim pkey As Long? = Convert.ToInt64(cmd.ExecuteScalar)

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return pkey > 0
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetLogFunctQuadMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "usp_FunctionalQuadriplegiaMessageLogGet"

                    cmd.Parameters.AddWithValue("RenderingNPI", RenderingNPI)
                    cmd.Parameters.AddWithValue("Year", Year)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    Dim pkey As Long? = Convert.ToInt64(cmd.ExecuteScalar)

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return pkey > 0
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function LogFunctQuadMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "usp_FunctionalQuadriplegiaMessageLog_Insert"

                    cmd.Parameters.AddWithValue("RenderingNPI", RenderingNPI)
                    cmd.Parameters.AddWithValue("Year", Year)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    cmd.ExecuteNonQuery()

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return True
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetLogInflammatoryPolyarthritisMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "usp_InflammatoryPolyarthritisMessage_Get"

                    cmd.Parameters.AddWithValue("RenderingNPI", RenderingNPI)
                    cmd.Parameters.AddWithValue("Year", Year)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    Dim pkey As Long? = Convert.ToInt64(cmd.ExecuteScalar)

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return pkey > 0
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function LogInflammatoryPolyarthritisMessage(ByVal RenderingNPI As String, ByVal Year As Short) As Boolean
        Try
            Using dbo As SqlDataObject = GetSqlObject()
                Using cmd As New SqlCommand()
                    cmd.Connection = dbo.Connection
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = "usp_InflammatoryPolyarthritisMessage_Insert"

                    cmd.Parameters.AddWithValue("RenderingNPI", RenderingNPI)
                    cmd.Parameters.AddWithValue("Year", Year)

                    If dbo.Connection.State <> ConnectionState.Open Then dbo.Connection.Open()

                    cmd.ExecuteNonQuery()

                    If dbo.Connection.State <> ConnectionState.Closed Then dbo.Connection.Close()

                    Return True
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Sub ErrorLog_Insert(ByVal errorLog As ErrorLog)

        Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

        Try
            Using cmd As New SqlCommand

                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspErrorLog_Insert"

                If errorLog.SessionID.HasValue Then cmd.Parameters.AddWithValue("SessionID", errorLog.SessionID)

                cmd.Parameters.AddWithValue("ClassName", errorLog.ClassName)
                cmd.Parameters.AddWithValue("Method", errorLog.Method)
                If Not String.IsNullOrEmpty(errorLog.MethodParameters) Then
                    cmd.Parameters.AddWithValue("MethodParameters", errorLog.MethodParameters)
                End If
                cmd.Parameters.AddWithValue("MemberID", errorLog.MemberID)
                cmd.Parameters.AddWithValue("RenderingNPI", errorLog.RenderingNPI)
                cmd.Parameters.AddWithValue("Message", errorLog.Message)
                cmd.Parameters.AddWithValue("StackTrace", errorLog.StrackTrace)
                cmd.Parameters.AddWithValue("RequestTimeSeconds", errorLog.RequestTimeSeconds)

                _DBO.ExecuteCommand(cmd)

            End Using

        Catch ex As Exception
            Throw
        End Try

    End Sub


    Public Function InsertDBDebugLog(debugLog As DebugLog) As Long
        Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

        Try
            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspDebugLog_Insert"

                cmd.Parameters.AddWithValue("logID", debugLog.LogID)
                cmd.Parameters.AddWithValue("sessionID", debugLog.SessionID)
                cmd.Parameters.AddWithValue("processID", debugLog.ProcessID)
                cmd.Parameters.AddWithValue("processDesc", debugLog.ProcessDesc)
                cmd.Parameters.AddWithValue("startDate", debugLog.StartDate)
                cmd.Parameters.AddWithValue("completeDate", debugLog.CompleteDate)
                cmd.Parameters.AddWithValue("dataContent", debugLog.DataContent)
                cmd.Parameters.AddWithValue("statusID", debugLog.StatusID)
                cmd.Parameters.AddWithValue("message", debugLog.Message)

                Dim LogID = CLng(_DBO.GetSingleValue(cmd))
                'Dim LogID = _DBO.ExecuteCommand(cmd)

                Return LogID
            End Using
        Catch ex As Exception
            Throw
        End Try

    End Function

    Public Function ValidateConcurrencyID(claimID As Long, concurrencyID As Long) As Integer
        Try
            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspClaim_ValidateConcurrencyID"

                cmd.Parameters.AddWithValue("claimID", claimID)
                cmd.Parameters.AddWithValue("concurrencyID", concurrencyID)

                Dim returnConcurrencyID = CInt(_DBO.GetSingleValue(cmd))

                Return returnConcurrencyID
            End Using
        Catch ex As Exception
            Dim methodname = MethodBase.GetCurrentMethod.Name
            Dim className = MethodBase.GetCurrentMethod.DeclaringType.Name
            Dim methodParams = MethodBase.GetCurrentMethod.GetParameters.ToString()
            Globals.LogError(0, className, methodname, methodParams, ex.Message, ex.StackTrace)
            Return -1
        End Try
    End Function

    Public Function VerifyMemberHasTHAForYear(MemberID As String, ClaimClass As Short, ClaimID As Long, AtHome As Boolean) As Boolean
        Dim response As Boolean = False
        Try
            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspAHA_VerifyMemberHasTHAForYear"

                cmd.Parameters.AddWithValue("MemberID", MemberID)
                cmd.Parameters.AddWithValue("ClaimClass", ClaimClass)
                cmd.Parameters.AddWithValue("ClaimID", ClaimID)
                cmd.Parameters.AddWithValue("AtHome", AtHome)

                response = CBool(_DBO.GetSingleValue(cmd))

            End Using
        Catch ex As Exception
            Dim methodname = MethodBase.GetCurrentMethod.Name
            Dim className = MethodBase.GetCurrentMethod.DeclaringType.Name
            Dim methodParams = MethodBase.GetCurrentMethod.GetParameters.ToString()
            Globals.LogError(0, className, methodname, methodParams, ex.Message, ex.StackTrace)
        End Try
        Return response
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Function AHAVerifySubProjectIsActive(projectName As String, year As Short) As Boolean
        Dim response As Boolean = False
        Try
            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "usp_AHAVerifySubProjectIsActive"

                cmd.Parameters.AddWithValue("projectName", projectName)
                cmd.Parameters.AddWithValue("Year", year)

                response = CBool(_DBO.GetSingleValue(cmd))
            End Using
        Catch ex As Exception
            Dim methodname = MethodBase.GetCurrentMethod.Name
            Dim className = MethodBase.GetCurrentMethod.DeclaringType.Name
            Dim methodParams = MethodBase.GetCurrentMethod.GetParameters.ToString()
            Globals.LogError(0, className, methodname, methodParams, ex.Message, ex.StackTrace)
        End Try
        Return response
    End Function

    Public Function VerifyIfFormExistsForDOS(model As FormHeaderSection, claimClass As Short, claimID As Long?) As Boolean
        Dim response As Boolean = False
        Try
            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspAHA_VerifyIfFormExistsForDOS"

                cmd.Parameters.AddWithValue("@memberID", model.MemberID.Value)
                cmd.Parameters.AddWithValue("@claimClass", claimClass)
                cmd.Parameters.AddWithValue("@DOS", model.DateOfVisit.Value)
                cmd.Parameters.AddWithValue("@claimID", claimID)

                response = CBool(_DBO.GetSingleValue(cmd))
            End Using
        Catch ex As Exception
            Dim methodname = MethodBase.GetCurrentMethod.Name
            Dim className = MethodBase.GetCurrentMethod.DeclaringType.Name
            Dim methodParams = MethodBase.GetCurrentMethod.GetParameters.ToString()
            Globals.LogError(0, className, methodname, methodParams, ex.Message, ex.StackTrace)
        End Try
        Return response
    End Function

    Public Function SaveClaimsSocialDeterminants2023(ByVal claimID As Long, ByVal socialdeterminants As SocialDeterminants2023, ByVal socialdeterminantsna As BooleanField, ByVal dbo As SqlDataObject) As Boolean

        Dim cmd As New SqlClient.SqlCommand

        Try

            If IsNothing(socialdeterminants) Then
                socialdeterminants = New SocialDeterminants2023()
            End If

            If socialdeterminants IsNot Nothing Then
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "uspSaveClaims_SocialDeterminants_2023"
                With cmd.Parameters
                    .AddWithValue("biClaimID", claimID)
                    .AddWithValue("IsAutosufficientInRequestForTransport", socialdeterminants.IsAutosufficientInRequestForTransport?.Value)
                    .AddWithValue("HasSafeRoof", socialdeterminants.HasSafeRoof?.Value)
                    .AddWithValue("HasSufficientFundsForFood", socialdeterminants.HasSufficientFundsForFood?.Value)
                    .AddWithValue("FeelSafeInLivingPlace", socialdeterminants.FeelSafeInLivingPlace?.Value)
                    .AddWithValue("socialdeterminants_na", IIf(socialdeterminantsna?.Value Is Nothing, False, socialdeterminantsna?.Value))
                End With
                dbo.ExecuteCommand(cmd)
            End If
        Catch ex As Exception
            'Throw
            _saveError = True
            Dim mp As New StringBuilder
            mp.AppendLine("Save AHA Detail, ClaimID: " & claimID.ToString())
            Globals.LogError(_sessionID, MethodBase.GetCurrentMethod.DeclaringType.Name, MethodBase.GetCurrentMethod.Name, mp.ToString, ex.Message.ToString(), ex.StackTrace.ToString())
        Finally
            cmd.Dispose()
        End Try
    End Function

    Public Function SaveAIInfo(model As AHAAIInfo) As Boolean
        Dim response As Boolean = False
        Try
            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "usp_AHAAIInfo_SaveInfo"

                cmd.Parameters.AddWithValue("@ClaimID", model.ClaimID)
                cmd.Parameters.AddWithValue("@ContractID", model.ContractID)
                cmd.Parameters.AddWithValue("@Transcript", model.Transcript)
                cmd.Parameters.AddWithValue("@HasFillAHA", model.HasFillAHA)

                _DBO.ExecuteCommand(cmd)

                response = True
            End Using
        Catch ex As Exception
            Dim methodname = MethodBase.GetCurrentMethod.Name
            Dim className = MethodBase.GetCurrentMethod.DeclaringType.Name
            Dim methodParams = MethodBase.GetCurrentMethod.GetParameters.ToString()
            Globals.LogError(0, className, methodname, methodParams, ex.Message, ex.StackTrace)
        End Try
        Return response
    End Function

    Public Function SaveAudio(model As AHAAIInfo) As Boolean
        Dim response As Boolean = False
        Try
            Dim _DBO As New SqlDataObject(ConnectionStrings(AppSettings("DBClaimAttach").ToString).ConnectionString, True)

            Using cmd As New SqlCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandText = "usp_AHAAIInfo_SaveAudio"

                cmd.Parameters.AddWithValue("@ClaimID", model.ClaimID)
                cmd.Parameters.AddWithValue("@ContractID", model.ContractID)
                cmd.Parameters.AddWithValue("@AudioBytes", model.AudioBytes)

                _DBO.ExecuteCommand(cmd)

                response = True
            End Using
        Catch ex As Exception
            Dim methodname = MethodBase.GetCurrentMethod.Name
            Dim className = MethodBase.GetCurrentMethod.DeclaringType.Name
            Dim methodParams = MethodBase.GetCurrentMethod.GetParameters.ToString()
            Globals.LogError(0, className, methodname, methodParams, ex.Message, ex.StackTrace)
        End Try
        Return response
    End Function

End Class