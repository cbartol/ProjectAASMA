using System;

namespace AASMAHoshimi
{
	public class CreateAgentAction : Action
	{
		public delegate void AgentCreatedDelegate(Type agentType);

		private AASMAAI ai;
		private Type agentType;
		private AgentCreatedDelegate onAgentCreated;
        private String username;

		public CreateAgentAction (AASMAAI ai, Type agentType, String username) {
			this.ai = ai;
			this.agentType = agentType;
            this.username = username;
		}
		public CreateAgentAction (AASMAAI ai, Type agentType, AgentCreatedDelegate onAgentCreated, String username)
		{
			this.ai = ai;
			this.agentType = agentType;
            this.username = username;
			this.onAgentCreated = onAgentCreated;
		}

		public void execute() {
			this.ai.getNanoBot().Build(agentType, username);
			if (onAgentCreated != null) {
				this.onAgentCreated (agentType);
			}
		}

		public void cancel() {
			// Nothing to do here
		}
	}
}

