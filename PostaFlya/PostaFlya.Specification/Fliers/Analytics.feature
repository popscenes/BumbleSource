Feature: Analytics
	In order to utilize the power of online presence
	As a paricipant who posts flier
	I want to be able view analytics related to my flier

Scenario: View Analytics
Given i have navigated to the CREATE PAGE for a FLIER TYPE Default
And I choose to enable Analytics
And I have 1000 Account Credits
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour Default
And the FLIER STATUS will be Active
And the FLIER will contain a FEATURE described as Gather Flier Analytics Feature with a cost of 200 credits
And A CREDIT TRANSACTION for 200 with description Gather Flier Analytics Feature will be created

