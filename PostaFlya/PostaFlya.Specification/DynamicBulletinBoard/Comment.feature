Feature: Comment on a FLIER
	As a browser in PARTICIPANT role
	I want to be able to comment on a FLIER

@mytag
Scenario: Comment On A Flier
	Given I have navigated to the public view page for a FLIER
	When I submit a comment on the FLIER
	Then the comment should be stored against the FLIER

Scenario: Comment On A Flier fails
	Given I have navigated to the public view page for a FLIER
	When I submit a comment on the FLIER that fails
	Then I should observe the failure "Comment Failed"

	

Scenario: View Flier Comments
	Given I have navigated to the public view page for a FLIER
	And There is a comment on the FLIER
	Then I should see the comments for the flier