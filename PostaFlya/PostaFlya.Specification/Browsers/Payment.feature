Feature: Payment
	In order to creater fliers with extra features
	As a browser
	I want to be pay to attach these features to my fliers

@mytag
Scenario: Accouunt Credit Page
	Given I am a BROWSER in PARTICIPANT ROLE
	When I go to the Add ACCOUUNT CREDIT PAGE
	Then I will be presented with the valid PAYMENT OPTIONS

Scenario: Add Credit TO Account
	Given I Am on the Add ACCOUUNT CREDIT PAGE
	When I go Select a PAYMENT OPTION
	Then I will be redirected to that OPTIONS PROCESS


Scenario: Payment Callback Success
	Given I Have Selected a PAYMENT OPTION
	When The Payment OPTION is Completed Successfully
	Then I will be Shown the Transaction Details
	And the my account will have the credit i purchased

Scenario: Payment Callback Failure
	Given I Have Selected a PAYMENT OPTION
	When The Payment OPTION is Completed Unsuccessfully
	Then I will be Shown the Error Details
	And the my account will not have the credit i purchased

Scenario: Payment Transaction History
	Given I have a Successful PAYMENT TRANSACTION
	And I have a Unuccessful PAYMENT TRANSACTION
	When I navigate to the TRANSACTION HISTORY PAGE
	Then I will be presented with My Transactions

#Scenario: View PaymentPending Fliers
#
#	And I Create Flier With With Insufficient Credit
#	When I navigate to the Pendng Fliers Page
#	Then I will be shown all the fliers that are PaymentPending Status
	
#Scenario: Pay For Pending Fliers
#	Given I am a BROWSER in PARTICIPANT ROLE
#	And I Create Flier With With Insufficient Credit
#	When I Add Credit To My Account
#	And I navigate to the Pendng Fliers Page
#	And I Choose to pay for a flier
#	Then I will no longer have fliers that are PaymentPending Status

#Scenario: Pay For Pending Fliers with analytics and No Credit
#	Given I am a BROWSER in PARTICIPANT ROLE
#	And i have navigated to the CREATE PAGE for a FLIER TYPE Default
#	And I choose to enable Analytics
#	And I have 0 Account Credits
#	And I SUBMIT the data for that FLIER 
#	When I navigate to the Pendng Fliers Page
#	And I Choose to pay for a flier
#	Then I will still have fliers that are PaymentPending Status and have the cost of

#Scenario: Pay for flier feature after create
#	Given I have created a FLIER with a FEATURE described as Gather Flier Analytics Feature with no credit
#	And I have 1000 Account Credits
#	When I navigate to the Pendng Fliers Page
#	And I Choose to pay for a flier
#	Then the FLIER will contain a FEATURE described as Gather Flier Analytics Feature 
