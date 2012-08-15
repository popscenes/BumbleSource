Feature: My Fliers Page
	As a BROWSER in PARTICIPANT role
	I want to be able to navigate to the my flier page
	So that i can see a list of FLIERS i have created 
	And I can view the details of each of those fliers.

@mytag
Scenario: Show Fliers I have Created
	Given I am a BROWSER in PARTICIPANT ROLE 
	When I navigate to the my fliers page 
	Then I should see a list of fliers I have created

Scenario: Show Fliers Detail View
	Given I am a BROWSER in PARTICIPANT ROLE  
	And I have navigated to the my fliers page 
	When I click on view for my FLIER 
	Then I should see the details for my FLIER
