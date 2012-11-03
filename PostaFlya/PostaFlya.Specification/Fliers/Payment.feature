Feature: Payment
	In order to creater fliers with extra features
	As a browser
	I want to be pay to attach these features to my fliers

@mytag
Scenario: Flier Payment Page
	Given I have a FLIER that requires payment
	When I go to the FLIER PAYMENT PAGE
	Then I will be presented with the valid PAYMENT OPTIONS
	And the FLIER COST

Scenario: Flier Payment
	Given I have a FLIER that requires payment
	And I Am on the FLIER PAYMENT PAGE
	When I go Select a PAYMENT OPTION
	Then I will be redirected to that OPTIONS PROCESS


Scenario: Flier Payment Callback Success
	Given I have a FLIER that requires payment
	And I Have Selected a PAYMENT OPTION
	When The Payment OPTION is Completed Successfully
	Then I will be Shown the Transaction Details

Scenario: Flier Payment Callback Failure
	Given I have a FLIER that requires payment
	And I Have Selected a PAYMENT OPTION
	When The Payment OPTION is Completed Unsuccessfully
	Then I will be Shown the Error Details

Scenario: Payment Transaction History
	Given I have a Successful PAYMENT TRANSACTION
	And I have a Unuccessful PAYMENT TRANSACTION
	When I navigate to the TRANSACTION HISTORY PAGE
	Then I will be presented with My Transactions
