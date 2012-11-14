Feature: PARTICIPANT can POST FLIER
	As a BROWSER in a PARTICIPANT role
	I want to be able to Edir a FLIER 
	so that the changes can be included in a DYNAMIC BULLETIN BOARD

Scenario: Edit Flier
Given I have created a FLIER 
When I navigate to the edit page for that FLIER and update any of the required data for a FLIER 
Then the FLIER will be updated to reflect those changes

Scenario: Edit Flier Add Contact Details Payment Option
Given I have created a FLIER 
When I navigate to the edit page for that FLIER and add default contact details for a FLIER 
Then the FLIER will be updated to reflect those changes
And contact details will be retrievable for the FLIER
And the FLIER will contain a PAYMENT OPTION for Added Contact Details
And the FLIER STATUS will be PaymentPending

Scenario: Edit Flier Remove Contact Details Payment Option
Given I have created a FLIER with Contact Details
When I navigate to the edit page for that FLIER and remove default contact details for a FLIER 
Then the FLIER will be updated to reflect those changes
And the FLIER will not contain a PAYMENT OPTION for Added Contact Details 
And the FLIER STATUS will be ACTIVE
