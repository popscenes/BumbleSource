Feature: PARTICIPANT can Edit FLIER
	As a BROWSER in a PARTICIPANT role
	I want to be able to Edir a FLIER 
	so that the changes can be included in a DYNAMIC BULLETIN BOARD

Scenario: Edit Flier
Given I have created a FLIER 
When I navigate to the edit page for that FLIER and update any of the required data for a FLIER 
Then the FLIER will be updated to reflect those changes

Scenario: Edit Flier Add TEAR OFF
Given I have created a FLIER 
And I have sufficient Account Credit
When I navigate to the edit page for that FLIER and add TEAR OFF to a FLIER 
Then the FLIER will be updated to reflect those changes
And the FLIER STATUS will be ACTIVE
And the FLIER will contain a FEATURE for TearOff in a enabled state with a cost of 80 credits

Scenario: Edit Flier RemoveTearOff
Given I have created a FLIER with TEAR OFF
When I navigate to the edit page for that FLIER and remove TEAR OFF to a FLIER 
Then the FLIER will be updated to reflect those changes
And the FLIER STATUS will be ACTIVE
And the FLIER will not contain a FEATURE for TearOff

Scenario: Edit Flier Add USER CONTACT
Given I have created a FLIER 
And I have sufficient Account Credit
When I navigate to the edit page for that FLIER and add USER CONTACT to a FLIER 
Then the FLIER will be updated to reflect those changes
And the FLIER STATUS will be ACTIVE
And the FLIER will contain a FEATURE for UserContact in a enabled state with a cost of 500 credits

Scenario: Edit Flier Remove USER CONTACT
Given I have created a FLIER with USER CONTACT
When I navigate to the edit page for that FLIER and remove USER CONTACT to a FLIER 
Then the FLIER will be updated to reflect those changes
And the FLIER STATUS will be ACTIVE
And the FLIER will not contain a FEATURE for UserContact
