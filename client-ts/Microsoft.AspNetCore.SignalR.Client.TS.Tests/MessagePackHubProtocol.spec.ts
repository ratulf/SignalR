import { MessagePackHubProtocol } from "../Microsoft.AspNetCore.SignalR.Client.TS/MessagePackHubProtocol"
import { MessageType, InvocationMessage } from "../Microsoft.AspnetCore.SignalR.Client.TS/IHubProtocol"

describe("MessageHubProtocol", () => {
    it("can write Invocation message", () => {
        let protocol = new MessagePackHubProtocol();
        let invocation = <InvocationMessage>{
            type: MessageType.Invocation,
            invocationId: "123",
            target: "myMethod",
            nonblocking: true,
            arguments: [42, true, "test", ["x1", "y2"], null]
        };

        var parsedMessages = protocol.parseMessages(protocol.writeMessage(invocation));
        expect(parsedMessages).toEqual([invocation]);
    });
});