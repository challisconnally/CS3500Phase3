README 

Comments/Issues/Bugs:
		In SubmitAssignmentText, there is a weird bug where it won't allow us to submit to an assignment if the
	assignment name has a hashtag in it. "Test 1" works while "Test #1" does not

		When a student submits, occasionally our code makes two identical versions of the submission 
	visible to the professor, doesn't affect functionality.

		When we try to look at submissions for an assignment as a professor, if there are 0 submissions, our query breaks
	due to some nullable syntax error, otherwise that aspect of our code works perfectly.

		We are unsure of our codes ability to determine when classes conflict due to time overlapping (CreateAssignment).