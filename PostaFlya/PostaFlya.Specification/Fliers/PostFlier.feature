Feature: PARTICIPANT can POST FLIER
	As a BROWSER in a PARTICIPANT role
	I want to be able to POST a FLIER 
	so that it can be included in a DYNAMIC BULLETIN BOARD

Scenario: Create Flier With Default Behaviour
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I have 1000 Account Credits
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be ACTIVE

Scenario: Create Flier With With Insufficient Credit
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I have 0 Account Credits
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be PaymentPending

Scenario: Attach Images to an existing flier
Given I have created a FLIER 
When I add images to the FLIER
Then The FLIER will contain the extra images

Scenario: Create Flier With User Contact With sufficient Credit
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to allow User Contact
And I have 1000 Account Credits
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be Active
And the FLIER will contain a FEATURE described as "People can send their contact details to you" with a cost of 500 credits

Scenario: Create Flier creates a Tiny Url for a flier
Given I have created a FLIER 
Then It should have a unique Tiny Url

