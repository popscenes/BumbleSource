Feature: Like
	As a BROWSER
	I want to be able to like various ENTITIES
	So that my like will be recorded

Scenario: Like Flier
Given I have navigated to the public view page for a FLIER
When I like that FLIER 
Then I will be recorded as having liked the flier once
And the FLIER likes will be incremented

Scenario: Cant Like Flier Twice 
Given I have navigated to the public view page for a FLIER
And I have liked a FLIER
When I like that FLIER 
Then I will be recorded as having liked the flier once
And the FLIER likes will remain the same

Scenario: View Flier Likes
Given I have navigated to the public view page for a FLIER 
And Someone has liked a FLIER
Then I should see the likes for the FLIER