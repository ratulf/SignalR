import { IHubProtocol, MessageType, HubMessage, InvocationMessage, ResultMessage, CompletionMessage } from "./IHubProtocol";
import { BinaryMessageFormat } from "./Formatters"

var msgpack = require("msgpack-lite");

export class MessagePackHubProtocol implements IHubProtocol {
    name(): string {
        return "messagepack";
    }

    parseMessages(input: ArrayBuffer): HubMessage[] {
        return BinaryMessageFormat.parse(input).map(m => this.parseMessage(m));
    }

    private parseMessage(input: Uint8Array): HubMessage {
        // msgpack encodes integer values up to 127 directly so no decoding required
        let messageType = input[0] as MessageType;
        switch (messageType) {
            case MessageType.Invocation:
                return this.createInvocationMessage(input);
            case MessageType.Result:
                return this.createStreamItemMessage(input);
            case MessageType.Completion:
                return this.createCompletionMessage(input);
            default:
                throw new Error("Invalid message type.");
        }
    }

    private createInvocationMessage(input: Uint8Array): InvocationMessage {
        // turn the message into a 4-element array
        input[0] = 0x94;
        let values = msgpack.decode(input);

        return {
            type: MessageType.Invocation,
            invocationId: values[0],
            nonblocking: values[1],
            target: values[2],
            arguments: values[3]
        } as InvocationMessage;
    }

    private createStreamItemMessage(input: Uint8Array): ResultMessage {
        // turn the message into a 4-element array
        input[0] = 0x92;
        let values = msgpack.decode(input);

        return {
            type: MessageType.Result,
            invocationId: values[0],
            item: values[1]
        } as ResultMessage;
    }

    private createCompletionMessage(input: Uint8Array): CompletionMessage {
        let offset = 1; // skip message type
        let invocationId = msgpack.decode(input.slice(offset));



        return {
            type: MessageType.Result,
            invocationId: invocationId,
            item: values[1]
        } as ResultMessage;
    }

    writeMessage(message: HubMessage): ArrayBuffer {
        switch (message.type) {
            case MessageType.Invocation:
                return this.writeInvocation(message as InvocationMessage);
            case MessageType.Result:
            case MessageType.Completion:
                throw new Error(`Writing messages of type '${message.type}' is not supported.`);
            default:
                throw new Error("Invalid message type.");
        }
    }

    private writeInvocation(invocationMessage: InvocationMessage): ArrayBuffer {
        let payload = msgpack.encode([ MessageType.Invocation, invocationMessage.invocationId,
            invocationMessage.nonblocking, invocationMessage.target, invocationMessage.arguments]);

        // slice to remove array header
        return BinaryMessageFormat.write(payload.slice(1));
    }
}