Feature: PARTICIPANT can POST FLIER
	As a BROWSER in a PARTICIPANT role
	I want to be able to POST a FLIER 
	so that it can be included in a DYNAMIC BULLETIN BOARD

Scenario: Create Flier With Default Behaviour
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be ACTIVE

Scenario: Attach Images to an existing flier
Given I have created a FLIER 
When I add images to the FLIER
Then The FLIER will contain the extra images

Scenario: Create Flier With Tear Off With Insufficient Credit
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to attach Tear Offs
And I dont have sufficient Account Credit
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be Active
And the FLIER will contain a FEATURE for TearOff in a disabled state with a cost of 2.00

Scenario: Create Flier With Tear Off With sufficient Credit
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to attach Tear Offs
And I have sufficient Account Credit
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be Active
And the FLIER will contain a FEATURE for TearOff in a enabled state with a cost of 2.00

Scenario: Create Flier With User Contact With Insufficient Credit
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to allow User Contact
And I dont have sufficient Account Credit
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be Active
And the FLIER will contain a FEATURE for UserContact in a disabled state with a cost of 5.00

Scenario: Create Flier With User Contact With sufficient Credit
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to allow User Contact
And I have sufficient Account Credit
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be Active
And the FLIER will contain a FEATURE for UserContact in a enabled state with a cost of 5.00